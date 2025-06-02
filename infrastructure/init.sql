CREATE TABLE IF NOT EXISTS sensors (
    id SERIAL PRIMARY KEY,
    machine_id VARCHAR(50),
    vibration FLOAT,
    temperature FLOAT,
    timestamp TIMESTAMP
);

CREATE TABLE IF NOT EXISTS production (
    id SERIAL PRIMARY KEY,
    machine_id VARCHAR(50),
    cycle_time FLOAT,
    man_time FLOAT,
    machine_time FLOAT,
    availability FLOAT,
    performance FLOAT,
    quality FLOAT,
    timestamp TIMESTAMP
);

CREATE TABLE IF NOT EXISTS predictions (
    id SERIAL PRIMARY KEY,
    machine_id VARCHAR(50),
    failure_probability FLOAT,
    timestamp TIMESTAMP
);
