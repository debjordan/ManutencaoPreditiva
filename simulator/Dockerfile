FROM python:3.9-slim
WORKDIR /app
COPY sensor_simulator.py .
RUN pip install paho-mqtt
ENV PYTHONUNBUFFERED=1
CMD ["python", "sensor_simulator.py"]
