use cgmath::{Vector3, InnerSpace};

#[no_mangle]
pub extern "C" fn compute_gravitational_acceleration(displacement: Vector3<f64>, m2: f64) -> Vector3<f64> {
    const GRAVITATIONAL_CONSTANT: f64 = 6.67430e-11;

    let distance = displacement.magnitude();

    let acceleration = displacement * (GRAVITATIONAL_CONSTANT * m2) / (distance * distance * distance);

    acceleration
}

#[cfg(test)]
mod tests {
    use super::*;
    use cgmath::assert_relative_eq;

    #[test]
    fn test_compute_gravitational_acceleration() {
        // acceleration of earth due to sun's gravity
        test_compute_gravitational_acceleration_once(
            Vector3::new(1.4960e11, 0.0, 0.0),
            1.98847e30,
            Vector3::new(5.9301e-3, 0.0, 0.0)
        );
        // acceleration of sun due to earth's gravity
        test_compute_gravitational_acceleration_once(
            -Vector3::new(1.4960e11, 0.0, 0.0),
            5.9722e24,
            -Vector3::new(1.7815e-8, 0.0, 0.0)
        );
    }
    
    fn test_compute_gravitational_acceleration_once(displacement: Vector3<f64>, m2: f64, expected: Vector3<f64>) {
        let actual = compute_gravitational_acceleration(displacement, m2);
        assert_relative_eq!(expected, actual, max_relative = 0.001);
    }
}
