
CREATE TABLE clients (
    client_id SERIAL PRIMARY KEY,
    username VARCHAR(50) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL, 
    email VARCHAR(100) NOT NULL
);

CREATE TABLE trainers (
    trainer_id SERIAL PRIMARY KEY,
    username VARCHAR(50) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL, 
    email VARCHAR(100) NOT NULL
);

-- hangi trainer hangi cliente atanmış -dy
CREATE TABLE assignments (
    assignment_id SERIAL PRIMARY KEY,
    trainer_id INT NOT NULL,
    client_id INT NOT NULL,
    FOREIGN KEY (trainer_id) REFERENCES trainers(trainer_id),
    FOREIGN KEY (client_id) REFERENCES clients(client_id),
    UNIQUE (trainer_id, client_id)
);

-- exercise logs
CREATE TABLE exercise_logs (
    log_id SERIAL PRIMARY KEY,
    client_id INT NOT NULL,
    exercise_type VARCHAR(50) NOT NULL,
    duration_minutes INT NOT NULL,
    sets INT,
    reps INT,
    calories_burned INT,
    log_date DATE NOT NULL,
    FOREIGN KEY (client_id) REFERENCES clients(client_id)
);

CREATE TABLE workout_programs (
    program_id SERIAL PRIMARY KEY,
    trainer_id INT NOT NULL,
    client_id INT NOT NULL,
    program_name VARCHAR(100) NOT NULL,
    description TEXT,
    created_date DATE NOT NULL,
    FOREIGN KEY (trainer_id) REFERENCES trainers(trainer_id),
    FOREIGN KEY (client_id) REFERENCES clients(client_id)
);

CREATE TABLE health_metrics (
    metric_id SERIAL PRIMARY KEY,
    client_id INT NOT NULL,
    weight_kg DECIMAL(5,2),
    heart_rate_bpm INT,
    sleep_hours DECIMAL(3,1),
    water_intake_liters DECIMAL(3,1),
    record_date DATE NOT NULL,
    FOREIGN KEY (client_id) REFERENCES clients(client_id)
);

CREATE TABLE fitness_goals (
    goal_id SERIAL PRIMARY KEY,
    client_id INT NOT NULL,
    goal_type VARCHAR(50) CHECK (goal_type IN ('weight_loss', 'strength_target', 'weekly_exercise_frequency')) NOT NULL,
    target_value DECIMAL(10,2) NOT NULL,
    current_value DECIMAL(10,2),
    deadline DATE,
    status VARCHAR(20) CHECK (status IN ('completed', 'in_progress', 'missed')),
    FOREIGN KEY (client_id) REFERENCES clients(client_id)
);

CREATE TABLE virtual_sessions (
    session_id SERIAL PRIMARY KEY,
    client_id INT NOT NULL,
    trainer_id INT NOT NULL,
    session_time TIMESTAMP NOT NULL,
    duration_minutes INT NOT NULL,
    status VARCHAR(20) CHECK (status IN ('scheduled', 'canceled', 'completed')) NOT NULL,
    FOREIGN KEY (client_id) REFERENCES clients(client_id),
    FOREIGN KEY (trainer_id) REFERENCES trainers(trainer_id)
);