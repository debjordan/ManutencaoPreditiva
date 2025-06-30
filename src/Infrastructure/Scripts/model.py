import pandas as pd
import numpy as np
from sklearn.ensemble import RandomForestClassifier
from sklearn.model_selection import train_test_split
import joblib

np.random.seed(42)
data = pd.DataFrame({
    'vibration': np.random.uniform(0.1, 2.0, 1000),
    'temperature': np.random.uniform(50.0, 100.0, 1000),
    'failure': np.random.choice([0, 1], 1000, p=[0.9, 0.1])
})

X = data[['vibration', 'temperature']]
y = data['failure']
X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.2, random_state=42)

model = RandomForestClassifier(n_estimators=100, random_state=42)
model.fit(X_train, y_train)

joblib.dump(model, 'model.pkl')
print("Modelo salvo em model.pkl")
