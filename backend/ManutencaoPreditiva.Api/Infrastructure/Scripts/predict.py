import sys
import joblib
import numpy as np

model = joblib.load('model.pkl')
vibration = float(sys.argv[1])
temperature = float(sys.argv[2])
X = np.array([[vibration, temperature]])
probability = model.predict_proba(X)[0][1]
print(probability)
