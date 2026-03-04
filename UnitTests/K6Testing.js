/**
 * Load Testing is primarily concerned with assessing the current performance of your system
 * in terms of concurrent users or requests per second.
 * When you want to understand if your system is meeting the performance goals, this is the type of test
 * you'll run.
 *
 * Run a load test to:
 *   - Assess the current performance of your system under typical and peak load
 *   - Make sure you are continuously meeting the performance standards as you make changes to your system
 *
 * Can be used to simulate a normal day in your business.
 */

import http from 'k6/http';
import { sleep } from 'k6';

export let options = {
    // Key performance thresholds (optional)
    thresholds: {
        http_req_duration: ['p(95)<500'], // 95% of requests must complete below 500ms
    },
    stages: [
        // Simulate a normal business day: ramp up to typical load, stay there, then ramp down
        { duration: '30s', target: 20 },   // morning ramp-up
        { duration: '90s', target: 20 },   // steady typical load
        { duration: '30s', target: 50 },   // lunch peak
        { duration: '120s', target: 50 },   // extended peak
        { duration: '30s', target: 10 },   // afternoon decline
        { duration: '60s', target: 0 },    // cooldown
    ],
    // Disable TLS verification for testing (if using self-signed certificates)
    insecureSkipTLSVerify: true,
    // Do not reuse connections to better simulate real user behavior
    noConnectionReuse: true,
};

const targetUrl = __ENV.TARGET_URL || 'http://localhost:8081/api/workout/APIWorkout/GetWorkoutInformation/1';
export default function () {
    http.get(targetUrl);
    // Think time: simulate user reading the page before next request (optional)
    sleep(1);
}