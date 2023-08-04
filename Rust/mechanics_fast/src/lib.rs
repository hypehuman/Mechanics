use cgmath::{InnerSpace, Vector3};

fn compute_gravitational_acceleration(displacement: Vector3<f32>, mass: f32) -> Vector3<f32> {
    let g: f32 = 9.81;

    let distance_squared = displacement.magnitude2();

    if distance_squared > 0.0 {
        let magnitude = g * mass / distance_squared.sqrt();
        displacement.normalize() * magnitude
    } else {
        Vector3::new(0.0, 0.0, 0.0)
    }
}

fn compute_acceleration(masses: &[f32], positions: &[Vector3<f32>], index_of_self: usize) -> Vector3<f32> {
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
