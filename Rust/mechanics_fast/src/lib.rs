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
