#!/bin/bash

./run-backend.sh &
BACKEND_PID=$!

./run-frontend.sh &
FRONTEND_PID=$!

echo "Backend corriendo con PID: $BACKEND_PID"
echo "Frontend corriendo con PID: $FRONTEND_PID"

wait $BACKEND_PID
wait $FRONTEND_PID
