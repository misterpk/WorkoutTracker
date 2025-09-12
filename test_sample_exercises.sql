-- Test sample of exercise data for validation
-- Sample of 5 exercises from the generated dataset

BEGIN TRANSACTION;

-- Clear existing exercise data if any (for testing only)
DELETE FROM Exercises;

-- Reset identity seed
DBCC CHECKIDENT ('Exercises', RESEED, 0);

INSERT INTO Exercises (Name, Description, PrimaryMuscle, CreatedAt, UpdatedAt) VALUES
    ('3/4 Sit-Up', 'Lie down on the floor and secure your feet. Your legs should be bent at the knees. Place your hands behind or to the side of your head. You will begin with your back on the ground. This will be your starting position. Flex your hips and spine to raise your torso toward your knees. At the top of the contraction your torso should be perpendicular to the ground. Reverse the motion, going only ¾ of the way down. Repeat for the recommended amount of repetitions.', 'Abdominals', GETDATE(), GETDATE());

INSERT INTO Exercises (Name, Description, PrimaryMuscle, CreatedAt, UpdatedAt) VALUES
    ('90/90 Hamstring', 'Lie on your back, with one leg extended straight out. With the other leg, bend the hip and knee to 90 degrees. You may brace your leg with your hands if necessary. This will be your starting position. Extend your leg straight into the air, pausing briefly at the top. Return the leg to the starting position. Repeat for 10-20 repetitions, and then switch to the other leg.', 'Hamstrings', GETDATE(), GETDATE());

INSERT INTO Exercises (Name, Description, PrimaryMuscle, CreatedAt, UpdatedAt) VALUES
    ('Ab Crunch Machine', 'Select a light resistance and sit down on the ab machine placing your feet under the pads provided and grabbing the top handles. Your arms should be bent at a 90 degree angle as you rest the triceps on the pads provided. This will be your starting position. At the same time, begin to lift the legs up as you crunch your upper torso. Breathe out as you perform this movement. Tip: Be sure to use a slow and controlled motion. Concentrate on using your abs to move the weight while relaxing your legs and feet. After a second pause, slowly return to the starting position as you breathe in. Repeat the movement for the prescribed amount of repetitions.', 'Abdominals', GETDATE(), GETDATE());

INSERT INTO Exercises (Name, Description, PrimaryMuscle, CreatedAt, UpdatedAt) VALUES
    ('Alternate Hammer Curl', 'Stand up with your torso upright and a dumbbell in each hand being held at arms length. The elbows should be close to the torso. The palms of the hands should be facing your torso. This will be your starting position. While holding the upper arm stationary, curl the right weight forward while contracting the biceps as you breathe out. Continue the movement until your biceps is fully contracted and the dumbbells are at shoulder level. Hold the contracted position for a second as you squeeze the biceps. Tip: Only the forearms should move. Slowly begin to bring the dumbbells back to starting position as your breathe in. Repeat the movement with the left hand. This equals one repetition. Continue alternating in this manner for the recommended amount of repetitions.', 'Biceps', GETDATE(), GETDATE());

INSERT INTO Exercises (Name, Description, PrimaryMuscle, CreatedAt, UpdatedAt) VALUES
    ('Bench Press', 'Lie back on a flat bench. Using a medium width grip (a grip that creates a 90-degree angle in the middle of the movement between the forearms and the upper arms), lift the bar from the rack and hold it straight over you with your arms locked. This will be your starting position. From the starting position, breathe in and begin coming down slowly until the bar touches your middle chest. After a brief pause, push the bar back to the starting position as you breathe out. Focus on pushing the bar using your chest muscles. Lock your arms and squeeze your chest in the contracted position at the top of the movement, hold for a second and then start coming down slowly again. Tip: Ideally, lowering the weight should take about twice as long as raising it. Repeat the movement for the prescribed amount of repetitions. When you are done, place the bar back in the rack.', 'Chest', GETDATE(), GETDATE());

-- Commit the transaction
COMMIT TRANSACTION;

-- Verification queries
SELECT COUNT(*) as TotalExercises FROM Exercises;
SELECT PrimaryMuscle, COUNT(*) as ExerciseCount FROM Exercises GROUP BY PrimaryMuscle ORDER BY ExerciseCount DESC;
SELECT * FROM Exercises ORDER BY Id;