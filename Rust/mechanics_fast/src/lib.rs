use cgmath::{Array, InnerSpace, Vector3};
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

#[no_mangle]
pub extern "C" fn pub_try_leap(
    requested_num_steps: usize,
    step_duration: f64,
    masses: *const f64,
    positions: *mut Vector3<f64>,
    velocities: *mut Vector3<f64>,
    num_bodies: usize,
) -> usize {
    let actual_num_steps = match num_bodies {
        3 => try_leap::<3>(
            requested_num_steps,
            step_duration,
            ptr_to_array_const(masses),
            ptr_to_array_mut(positions),
            ptr_to_array_mut(velocities),
        ),
        60 => try_leap::<60>(
            requested_num_steps,
            step_duration,
            ptr_to_array_const(masses),
            ptr_to_array_mut(positions),
            ptr_to_array_mut(velocities),
        ),
        2048 => try_leap::<2048>(
            requested_num_steps,
            step_duration,
            ptr_to_array_const(masses),
            ptr_to_array_mut(positions),
            ptr_to_array_mut(velocities),
        ),
        _ => 0 // TODO: Return error message: format!("Unsupported num_bodies: {}", num_bodies)
    };
    actual_num_steps
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

fn try_compute_step<const N: usize>(
    step_duration: f64,
    masses: &[f64; N],
    positions_curr: &[Vector3<f64>; N],
    velocities_curr: &[Vector3<f64>; N],
    positions_next: &mut [Vector3<f64>; N],
    velocities_next: &mut [Vector3<f64>; N],
    accelerations: &mut [Vector3<f64>; N],
) -> bool {
    compute_gravitational_acceleration_many_on_many(masses, positions_curr, accelerations);
    // C# code parallelizes this loop, but I don't know how to do that in Rust while mutating two arrays.
    for i in 0..N {
        let a = accelerations[i];
        let v = velocities_curr[i] + step_duration * a;
        let p = positions_curr[i] + step_duration * v;
        if !a.is_finite() || !v.is_finite() || !p.is_finite() {
            return false;
        }
        positions_next[i] = p;
        velocities_next[i] = v;
    }
    true
}

fn try_leap<const N: usize>(
    requested_num_steps: usize,
    step_duration: f64,
    masses: &[f64; N],
    positions: &mut [Vector3<f64>; N],
    velocities: &mut [Vector3<f64>; N],
) -> usize {
    // Alternate between using one set as the "read" arrays and the other set as the "write" arrays.
    // Technially one of these could be local to try_compute_step.
    let positions_b = &mut [ZERO_VECTOR; N];
    let velocities_b = &mut [ZERO_VECTOR; N];

    // Alternatively, we could initialize one of these in each try_compute_step.
    // I'm betting that declaring it here and reusing it in each step will give better performance,
    // but let's see about that.
    let accelerations = &mut [ZERO_VECTOR; N];

    let mut actual_num_steps = requested_num_steps;
    for step_i in 0..requested_num_steps {
        let success: bool;
        if step_i % 2 == 0 {
            success = try_compute_step(step_duration, masses, positions, velocities, positions_b, velocities_b, accelerations);
        }
        else {
            success = try_compute_step(step_duration, masses, positions_b, velocities_b, positions, velocities, accelerations);
        };
        if !success {
            actual_num_steps = step_i;
            break
        }
    }

    // If there was an odd number of steps, we need to copy the "b" values back to the original arrays.
    if actual_num_steps % 2 == 1 {
        for body_i in 0..N {
            positions[body_i] = positions_b[body_i];
            velocities[body_i] = velocities_b[body_i];
        }
    }

    actual_num_steps
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

    const EARTH_ORBIT_SUN_SPEED: f64 = 2.978e4;
    const MOON_ORBIT_EARTH_SPEED: f64 = 1.022e3;
    const SUN_VELOCITY: Vector3<f64> = ZERO_VECTOR;
    const EARTH_VELOCITY: Vector3<f64> = Vector3::new(0.0, EARTH_ORBIT_SUN_SPEED, 0.0);
    const MOON_VELOCITY: Vector3<f64> = Vector3::new(-MOON_ORBIT_EARTH_SPEED, EARTH_ORBIT_SUN_SPEED, 0.0);
    const THREE_VELOCITIES: [Vector3<f64>; 3] = [SUN_VELOCITY, EARTH_VELOCITY, MOON_VELOCITY];

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

    #[test]
    fn test_try_leap_success() {
        // movement over time of the sun, earth, and moon under the influence of one another's gravity
        const N: usize = 3; // number of bodies
        let expected_positions = [
            Vector3::new(6.924e-05, 2.456e-09, 0.000e00),
            Vector3::new(1.496e11, 2.382e06, 0.000e00),
            Vector3::new(1.496e11, 3.868e08, 0.000e00),
        ];
        let expected_velocities = [
            Vector3::new(1.442e-06, 5.423e-11, 0.000e00),
            Vector3::new(-4.744e-01, 2.978e04, 0.000e00),
            Vector3::new(-1.022e03, 2.978e04, 0.000e00),
        ];

        let requested_num_steps = 5;
        let step_duration = 16.0;
        let mut positions = THREE_POSITIONS;
        let mut velocities = THREE_VELOCITIES;
        let actual_num_steps = try_leap(requested_num_steps, step_duration, &THREE_MASSES, &mut positions, &mut velocities);

        assert_eq!(requested_num_steps, actual_num_steps);
        for i in 0..N {
            assert_relative_eq!(expected_positions[i], positions[i], max_relative = 0.001);
            assert_relative_eq!(expected_velocities[i], velocities[i], max_relative = 0.001);
        }
    }

    #[test]
    fn test_try_leap_error() {
        // Make sure that non-real results cause the step to fail.
        const N: usize = 2; // number of bodies

        let requested_num_steps = 3;
        let masses = [1.0; N];
        let mut positions = [ZERO_VECTOR; N];
        let mut velocities = [ZERO_VECTOR; N];
        let actual_num_steps = try_leap(requested_num_steps, 1.0, &masses, &mut positions, &mut velocities);

        assert_eq!(0, actual_num_steps);
        for i in 0..N {
            assert_eq!(ZERO_VECTOR, positions[i]);
            assert_eq!(ZERO_VECTOR, velocities[i]);
        }
    }
}
