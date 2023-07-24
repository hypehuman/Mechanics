use cgmath::{Vector3, InnerSpace};

#[no_mangle]
pub extern "C" fn compute_gravitational_acceleration(displacement: Vector3<f64>, m2: f64) -> Vector3<f64> {
    const GRAVITATIONAL_CONSTANT: f64 = 6.67430e-11;

    let distance = displacement.magnitude();

    let acceleration = displacement * (GRAVITATIONAL_CONSTANT * m2) / (distance * distance * distance);

    acceleration
}

struct Vector3D {
    x: f32,
    y: f32,
    z: f32,
}

fn compute_gravitational_acceleration(displacement: Vector3D, mass: f32) -> Vector3D {
    let g: f32 = 9.81;

    let distance_squared = displacement.x.powi(2) + displacement.y.powi(2) + displacement.z.powi(2);

    if distance_squared > 0.0 {
        let magnitude = g * mass / distance_squared.sqrt();
        Vector3D {
            x: magnitude * displacement.x / distance_squared.sqrt(),
            y: magnitude * displacement.y / distance_squared.sqrt(),
            z: magnitude * displacement.z / distance_squared.sqrt(),
        }
    } else {
        Vector3D { x: 0.0, y: 0.0, z: 0.0 }
    }
}

fn compute_acceleration(masses: &[f32], positions: &[Vector3D], index_of_self: usize) -> Vector3D {
    let mut acceleration = Vector3D { x: 0.0, y: 0.0, z: 0.0 };

    for i in 0..masses.len() {
        if i != index_of_self {
            let displacement = Vector3D {
                x: positions[i].x - positions[index_of_self].x,
                y: positions[i].y - positions[index_of_self].y,
                z: positions[i].z - positions[index_of_self].z,
            };
            let grav_acceleration = compute_gravitational_acceleration(displacement, masses[i]);
            acceleration.x += grav_acceleration.x;
            acceleration.y += grav_acceleration.y;
            acceleration.z += grav_acceleration.z;
        }
    }

    acceleration
}
