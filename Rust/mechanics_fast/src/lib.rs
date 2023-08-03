use cgmath::{InnerSpace, Vector3};

#[no_mangle]
pub extern "C" fn compute_gravitational_acceleration(displacement: Vector3<f64>, m2: f64) -> Vector3<f64> {
    const GRAVITATIONAL_CONSTANT: f64 = 6.67430e-11;

    let distance = displacement.magnitude();

    let acceleration = displacement * (GRAVITATIONAL_CONSTANT * m2) / (distance * distance * distance);
    acceleration
}

#[no_mangle]
pub extern "C" fn compute_acceleration(masses: &[f64], positions: &[Vector3<f64>], index_of_self: usize) -> Vector3<f64> {
    let mut acceleration = Vector3::new(0.0, 0.0, 0.0);

    for i in 0..masses.len() {
        if i != index_of_self {
            let displacement = positions[i] - positions[index_of_self];
            let grav_acceleration = compute_gravitational_acceleration(displacement, masses[i]);
            acceleration += grav_acceleration;
        }
    }

    acceleration
}

#[cfg(test)]
mod tests {
    use super::*;
    use cgmath::assert_relative_eq;

    const SUN_MASS:f64 = 1.98847e30;
    const EARTH_MASS:f64 = 5.9722e24;
    const MOON_MASS:f64 = 7.34767309e22;

    const SUN_EARTH_DISTANCE:f64 = 1.4960e11;
    const EARTH_MOON_DISTANCE:f64 = 3.84399e8;

    #[test]
    fn test_compute_gravitational_acceleration() {
        // acceleration of earth due to sun's gravity
        test_compute_gravitational_acceleration_once(
            Vector3::new(SUN_EARTH_DISTANCE, 0.0, 0.0),
            SUN_MASS,
            Vector3::new(5.9301e-3, 0.0, 0.0),
        );
        // acceleration of sun due to earth's gravity
        test_compute_gravitational_acceleration_once(
            -Vector3::new(SUN_EARTH_DISTANCE, 0.0, 0.0),
            EARTH_MASS,
            -Vector3::new(1.7815e-8, 0.0, 0.0),
        );
    }

    fn test_compute_gravitational_acceleration_once(displacement: Vector3<f64>, m2: f64, expected: Vector3<f64>) {
        let actual = compute_gravitational_acceleration(displacement, m2);
        assert_relative_eq!(expected, actual, max_relative = 0.001);
    }

    #[test]
    fn test_compute_acceleration() {
        let masses: &[f64] = &[SUN_MASS, EARTH_MASS, MOON_MASS];
        let positions: &[Vector3<f64>] = &[
            Vector3::new(0.0, 0.0, 0.0), // place sun at origin
            Vector3::new(SUN_EARTH_DISTANCE, 0.0, 0.0), // place earth in direction +x from sun
            Vector3::new(SUN_EARTH_DISTANCE, EARTH_MOON_DISTANCE, 0.0), // place moon in direction +y from earth
        ];

        let expected_accelerations:[Vector3<f64>; 3] = [
            Vector3::new(1.8030e-08, 5.6304e-13, 0.0000e00),
            Vector3::new(-5.9301e-03, 3.3189e-05, 0.0000e00),
            Vector3::new(-5.9300e-03, -2.7128e-03, 0.0000e00),
        ];

        for i in 0..3 {
            test_compute_acceleration_once(masses, positions, i, expected_accelerations[i]);
        }
    }

    fn test_compute_acceleration_once(masses: &[f64], positions: &[Vector3<f64>], index_of_self: usize, expected: Vector3<f64>) {
        let actual = compute_acceleration(masses, positions, index_of_self);
        assert_relative_eq!(expected, actual, max_relative = 0.001);
    }
}
