CREATE TABLE sensors (
    id SERIAL PRIMARY KEY,
    machine_id VARCHAR(50),
    vibration FLOAT,
    temperature FLOAT,
    timestamp TIMESTAMP
);

CREATE TABLE production (
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

CREATE TABLE predictions (
    id SERIAL PRIMARY KEY,
    machine_id VARCHAR(50),
    failure_probability FLOAT,
    timestamp TIMESTAMP
);
