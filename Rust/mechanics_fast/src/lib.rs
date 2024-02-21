use cgmath::{InnerSpace, Vector3};
use rayon::prelude::*;

const ZERO_VECTOR: Vector3<f64> = Vector3::new(0.0, 0.0, 0.0);

#[no_mangle]
pub extern "C" fn pub_compute_gravitational_acceleration_one_on_one(
    displacement: Vector3<f64>,
    m2: f64,
) -> Vector3<f64> {
    compute_gravitational_acceleration_one_on_one(displacement, m2)
}

#[no_mangle]
pub extern "C" fn pub_compute_gravitational_acceleration_many_on_one(
    masses: *const f64,
    positions: *const Vector3<f64>,
    num_bodies: usize,
    index_of_self: usize,
) -> Vector3<f64> {
    match num_bodies {
        3 => compute_gravitational_acceleration_many_on_one::<3>(
            ptr_to_array_const(masses),
            ptr_to_array_const(positions),
            index_of_self,
        ),
        60 => compute_gravitational_acceleration_many_on_one::<60>(
            ptr_to_array_const(masses),
            ptr_to_array_const(positions),
            index_of_self,
        ),
        2048 => compute_gravitational_acceleration_many_on_one::<2048>(
            ptr_to_array_const(masses),
            ptr_to_array_const(positions),
            index_of_self,
        ),
        _ => panic!("Unsupported num_bodies: {}", num_bodies)
    }
}

#[no_mangle]
pub extern "C" fn pub_compute_gravitational_acceleration_many_on_many(
    masses: *const f64,
    positions: *const Vector3<f64>,
    num_bodies: usize,
    accelerations: *mut Vector3<f64>,
) {
    match num_bodies {
        3 => compute_gravitational_acceleration_many_on_many::<3>(
            ptr_to_array_const(masses),
            ptr_to_array_const(positions),
            ptr_to_array_mut(accelerations),
        ),
        60 => compute_gravitational_acceleration_many_on_many::<60>(
            ptr_to_array_const(masses),
            ptr_to_array_const(positions),
            ptr_to_array_mut(accelerations),
        ),
        2048 => compute_gravitational_acceleration_many_on_many::<2048>(
            ptr_to_array_const(masses),
            ptr_to_array_const(positions),
            ptr_to_array_mut(accelerations),
        ),
        _ => panic!("Unsupported num_bodies: {}", num_bodies)
    }
}

fn ptr_to_array_const<T, const N: usize>(ptr: *const T) -> &'static [T; N] {
    let slice = unsafe { std::slice::from_raw_parts(ptr, N) };
    let array = slice.try_into().expect("invalid size");
    array
}

fn ptr_to_array_mut<T, const N: usize>(ptr: *mut T) -> &'static mut [T; N] {
    let slice = unsafe { std::slice::from_raw_parts_mut(ptr, N) };
    let array = slice.try_into().expect("invalid size");
    array
}

fn compute_gravitational_acceleration_one_on_one(
    displacement: Vector3<f64>,
    m2: f64,
) -> Vector3<f64> {
    const GRAVITATIONAL_CONSTANT: f64 = 6.67430e-11;

    let distance = displacement.magnitude();

    let acceleration = displacement * (GRAVITATIONAL_CONSTANT * m2) / (distance * distance * distance);
    acceleration
}

fn compute_gravitational_acceleration_many_on_one<const N: usize>(
    masses: &[f64; N],
    positions: &[Vector3<f64>; N],
    index_of_self: usize,
) -> Vector3<f64> {
    let mut acceleration = ZERO_VECTOR;

    for i in 0..N {
        if i != index_of_self {
            let displacement = positions[i] - positions[index_of_self];
            let grav_acceleration = compute_gravitational_acceleration_one_on_one(displacement, masses[i]);
            acceleration += grav_acceleration;
        }
    }

    acceleration
}

fn compute_gravitational_acceleration_many_on_many<const N: usize>(
    masses: &[f64; N],
    positions: &[Vector3<f64>; N],
    accelerations: &mut [Vector3<f64>; N],
) {
    accelerations.par_iter_mut().enumerate().for_each(|(i, a)| {
        *a = compute_gravitational_acceleration_many_on_one(masses, positions, i);
    });
}

#[cfg(test)]
mod tests {
    use super::*;
    use cgmath::assert_relative_eq;

    const SUN_MASS: f64 = 1.9885e30;
    const EARTH_MASS: f64 = 5.9724e24;
    const MOON_MASS: f64 = 7.3476e22;
    const THREE_MASSES: [f64; 3] = [SUN_MASS, EARTH_MASS, MOON_MASS];

    const SUN_EARTH_DISTANCE: f64 = 1.4960e11;
    const EARTH_MOON_DISTANCE: f64 = 3.84399e8;

    const SUN_POSITION: Vector3<f64> = ZERO_VECTOR; // place sun at origin
    const EARTH_POSITION: Vector3<f64> = Vector3::new(SUN_EARTH_DISTANCE, 0.0, 0.0); // place earth in direction +x from sun
    const MOON_POSITION: Vector3<f64> = Vector3::new(SUN_EARTH_DISTANCE, EARTH_MOON_DISTANCE, 0.0); // place moon in direction +y from earth
    const THREE_POSITIONS: [Vector3<f64>; 3] = [SUN_POSITION, EARTH_POSITION, MOON_POSITION];

    #[test]
    fn test_compute_gravitational_acceleration_one_on_one() {
        // acceleration of earth due to sun's gravity
        let expected = Vector3::new(5.9301e-3, 0.0, 0.0);
        let actual = compute_gravitational_acceleration_one_on_one(EARTH_POSITION, SUN_MASS);
        assert_relative_eq!(expected, actual, max_relative = 0.001);
    }

    #[test]
    fn test_compute_gravitational_acceleration_many_on_one() {
        // acceleration of earth due to both sun's and moon's gravity
        let expected = Vector3::new(-5.9301e-03, 3.3188e-05, 0.0000e00);
        let actual = compute_gravitational_acceleration_many_on_one(&THREE_MASSES, &THREE_POSITIONS, 1);
        assert_relative_eq!(expected, actual, max_relative = 0.001);
    }

    #[test]
    fn test_compute_gravitational_acceleration_many_on_many() {
        // acceleration of the sun, earth, and moon due to one another
        const N: usize = 3; // number of bodies
        let expected = [
            Vector3::new(1.8030e-08, 5.6303e-13, 0.0000e00),
            Vector3::new(-5.9301e-03, 3.3188e-05, 0.0000e00),
            Vector3::new(-5.9301e-03, -2.7129e-03, 0.0000e00),
        ];

        let mut actual = [ZERO_VECTOR; N];
        compute_gravitational_acceleration_many_on_many(&THREE_MASSES, &THREE_POSITIONS, &mut actual);

        for i in 0..N {
            assert_relative_eq!(expected[i], actual[i], max_relative = 0.001);
        }
    }
}
