use cgmath::{InnerSpace, Vector3};

#[no_mangle]
pub extern "C" fn pub_compute_gravitational_acceleration_one_on_one(displacement: Vector3<f64>, m2: f64) -> Vector3<f64> {
    compute_gravitational_acceleration_one_on_one(displacement, m2)
}

#[no_mangle]
pub extern "C" fn pub_compute_gravitational_acceleration_many_on_one(masses: *const f64, positions: *const Vector3<f64>, num_bodies: usize, index_of_self: usize) -> Vector3<f64> {
    let masses_slice = unsafe { std::slice::from_raw_parts(masses, num_bodies) };
    let positions_slice = unsafe { std::slice::from_raw_parts(positions, num_bodies) };
    let acceleration = compute_gravitational_acceleration_many_on_one(masses_slice, positions_slice, index_of_self);

    acceleration
}

fn compute_gravitational_acceleration_one_on_one(displacement: Vector3<f64>, m2: f64) -> Vector3<f64> {
    const GRAVITATIONAL_CONSTANT: f64 = 6.67430e-11;

    let distance = displacement.magnitude();

    let acceleration = displacement * (GRAVITATIONAL_CONSTANT * m2) / (distance * distance * distance);
    acceleration
}

fn compute_gravitational_acceleration_many_on_one(masses: &[f64], positions: &[Vector3<f64>], index_of_self: usize) -> Vector3<f64> {
    let mut acceleration = Vector3::new(0.0, 0.0, 0.0);

    for i in 0..masses.len() {
        if i != index_of_self {
            let displacement = positions[i] - positions[index_of_self];
            let grav_acceleration = compute_gravitational_acceleration_one_on_one(displacement, masses[i]);
            acceleration += grav_acceleration;
        }
    }

    acceleration
}

#[cfg(test)]
mod tests {
    use super::*;
    use cgmath::assert_relative_eq;

    const SUN_MASS:f64 = 1.9885e30;
    const EARTH_MASS:f64 = 5.9724e24;
    const MOON_MASS:f64 = 7.3476e22;

    const SUN_EARTH_DISTANCE:f64 = 1.4960e11;
    const EARTH_MOON_DISTANCE:f64 = 3.84399e8;

    #[test]
    fn test_compute_gravitational_acceleration_one_on_one() {
        // acceleration of earth due to sun's gravity
        compute_gravitational_acceleration_one_on_one_once(
            Vector3::new(SUN_EARTH_DISTANCE, 0.0, 0.0),
            SUN_MASS,
            Vector3::new(5.9301e-3, 0.0, 0.0),
        );
        // acceleration of sun due to earth's gravity
        compute_gravitational_acceleration_one_on_one_once(
            -Vector3::new(SUN_EARTH_DISTANCE, 0.0, 0.0),
            EARTH_MASS,
            -Vector3::new(1.7815e-8, 0.0, 0.0),
        );
    }

    fn compute_gravitational_acceleration_one_on_one_once(displacement: Vector3<f64>, m2: f64, expected: Vector3<f64>) {
        let actual = compute_gravitational_acceleration_one_on_one(displacement, m2);
        assert_relative_eq!(expected, actual, max_relative = 0.001);
    }

    #[test]
    fn test_compute_gravitational_acceleration_many_on_one() {
        let masses: &[f64] = &[SUN_MASS, EARTH_MASS, MOON_MASS];
        let positions: &[Vector3<f64>] = &[
            Vector3::new(0.0, 0.0, 0.0), // place sun at origin
            Vector3::new(SUN_EARTH_DISTANCE, 0.0, 0.0), // place earth in direction +x from sun
            Vector3::new(SUN_EARTH_DISTANCE, EARTH_MOON_DISTANCE, 0.0), // place moon in direction +y from earth
        ];

        let expected_accelerations:[Vector3<f64>; 3] = [
            Vector3::new(1.8030e-08, 5.6303e-13, 0.0000e00),
            Vector3::new(-5.9301e-03, 3.3188e-05, 0.0000e00),
            Vector3::new(-5.9301e-03, -2.7129e-03, 0.0000e00),
        ];

        for i in 0..3 {
            test_compute_gravitational_acceleration_many_on_one_once(masses, positions, i, expected_accelerations[i]);
        }
    }

    fn test_compute_gravitational_acceleration_many_on_one_once(masses: &[f64], positions: &[Vector3<f64>], index_of_self: usize, expected: Vector3<f64>) {
        let actual = compute_gravitational_acceleration_many_on_one(masses, positions, index_of_self);
        assert_relative_eq!(expected, actual, max_relative = 0.001);
    }
}
