import http from 'k6/http';
import { check, sleep } from 'k6';

// Test configuration
export const options = {
    // Determining how many requests K6 should produce per second.
    // Since our rate limiter allows 20 requests per 10 seconds (~2 per second), we will send 10 requests per second to exceed the limit.
    scenarios: {
        constant_request_rate: {
            executor: 'constant-arrival-rate',
            rate: 10,           // Generate 10 requests per second
            timeUnit: '1s',
            duration: '15s',
            preAllocatedVUs: 1, // Keep at least 1 virtual user ready at all times
            maxVUs: 10,         // Can scale up to 10 VUs if needed
        },
    },
};

// The Test Setup
export default function () {
    // The API Gateway address and a random GET request that we load balance
    // (In a real scenario, an ID created via POST could be used instead)
    const url = 'http://host.docker.internal:5024/basket-api/api/baskets/00000000-0000-0000-0000-000000000000';
    
    const res = http.get(url);

    // Asserting the results with K6
    check(res, {
        'Status Code is 200? (Successful)': (r) => r.status === 200,
        'Status Code is 404? (Basket Not Found - Normal)': (r) => r.status === 404, // It will return 404 because the ID is not in the DB, which is normal and means the Gateway allowed it.
        'Status Code is 429? (Rate Limit Exceeded!)': (r) => r.status === 429,
    });
}
