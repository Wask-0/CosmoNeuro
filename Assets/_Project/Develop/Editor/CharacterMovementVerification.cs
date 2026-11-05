using System;
using _Project.Develop.Runtime.Gameplay.Characters.Movements;
using _Project.Develop.Runtime.Gameplay.Characters.Movements.Movement;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.Develop.Editor
{
    public static class CharacterMovementVerification
    {
        [MenuItem("Tools/The Lost Cosmonaut/Verify Character Movement")]
        public static void Verify()
        {
            if (EditorApplication.isPlaying)
                throw new InvalidOperationException("Stop Play Mode before running verification.");
            VerifyInertia();
            VerifyMotor();
            Debug.Log("CHARACTER_MOVEMENT_VERIFICATION_OK: inertia, reversal, floor, thin wall, departure from wall; Rigidbody2D.");
        }

        private static void VerifyInertia()
        {
            ProbeMotor motor = new ProbeMotor();
            DirectionalMover mover = new DirectionalMover(motor, 5f, 15f, 6f, 20f);
            mover.Move(1f);
            mover.Update(0.1f);
            Require(Mathf.Abs(motor.CurrentVelocity.x - 1.5f) < 0.001f, "Acceleration must be gradual.");
            for (int i = 0; i < 10; i++) mover.Update(0.1f);
            Require(Mathf.Abs(motor.CurrentVelocity.x - 5f) < 0.001f, "Maximum speed.");
            mover.Move(0f);
            mover.Update(0.1f);
            Require(Mathf.Abs(motor.CurrentVelocity.x - 4.4f) < 0.001f, "Release must preserve inertia.");
            for (int i = 0; i < 10; i++) mover.Update(0.1f);
            Require(motor.CurrentVelocity.x == 0f, "Complete stop.");
            motor.SetHorizontalVelocity(1f);
            mover.Move(-1f);
            mover.Update(0.1f);
            Require(Mathf.Abs(motor.CurrentVelocity.x + 0.75f) < 0.001f, "Reversal must use remaining acceleration time.");
        }

        private static void VerifyMotor()
        {
            Scene scene = SceneManager.CreateScene("Movement Verification", new CreateSceneParameters(LocalPhysicsMode.Physics2D));
            PhysicsMaterial2D material = new PhysicsMaterial2D("Verification") { friction = 0f, bounciness = 0f };
            try
            {
                GameObject floor = InScene("Floor", scene, new Vector2(0f, -0.25f));
                floor.AddComponent<BoxCollider2D>().size = new Vector2(30f, 0.5f);
                GameObject wall = InScene("Thin Wall", scene, new Vector2(3f, 2.5f));
                wall.AddComponent<BoxCollider2D>().size = new Vector2(0.1f, 5f);
                GameObject character = InScene("Character", scene, new Vector2(0f, 1.6f));
                Rigidbody2D body = character.AddComponent<Rigidbody2D>();
                CapsuleCollider2D collider = character.AddComponent<CapsuleCollider2D>();
                collider.size = new Vector2(0.8f, 1.6f);
                collider.sharedMaterial = material;
                ICharacterMotor motor = new RigidbodyCharacterMotor(body, 0.3f);
                PhysicsScene2D physics = scene.GetPhysicsScene2D();
                physics.Simulate(0.02f);
                for (int i = 0; i < 200; i++) Step(motor, physics, 0f);
                Require(body.position.y > 0.76f && body.position.y < 0.86f, "Character must rest on the floor: " + body.position);
                // Cross more than the wall thickness each frame.
                for (int i = 0; i < 20; i++) Step(motor, physics, 30f);
                Require(body.position.x <= 2.58f && body.position.x > 2.3f, "Thin wall must block fast movement: " + body.position);
                Require(Mathf.Abs(motor.CurrentVelocity.x) < 0.1f, "Blocked velocity must be cleared.");
                for (int i = 0; i < 15; i++) Step(motor, physics, -5f);
                Require(body.position.x < 1.5f, "Character must leave the wall when reversing.");
                Require(body.position.y > 0.76f && body.position.y < 0.86f, "Horizontal movement must preserve floor contact.");
                Debug.Log("Motor verified: Rigidbody2D");
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
                UnityEngine.Object.DestroyImmediate(material);
            }
        }

        private static void Step(ICharacterMotor motor, PhysicsScene2D physics, float velocity)
        {
            motor.SetHorizontalVelocity(velocity);
            motor.Update(0.02f);
            physics.Simulate(0.02f);
        }

        private static GameObject InScene(string name, Scene scene, Vector2 position)
        {
            GameObject item = new GameObject(name);
            SceneManager.MoveGameObjectToScene(item, scene);
            item.transform.position = position;
            return item;
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }

        private sealed class ProbeMotor : ICharacterMotor
        {
            public Vector2 CurrentVelocity { get; private set; }
            public bool IsGrounded => true;
            public float GravityMagnitude => 0f;
            public void SetHorizontalVelocity(float velocity) => CurrentVelocity = new Vector2(velocity, 0f);
            public void SetVerticalVelocity(float velocity) { }
            public void SetSimulated(bool simulated) { }
            public void Update(float deltaTime) { }
        }
    }
}
