import http from 'k6/http';
import { check, sleep } from 'k6';
import { SharedArray } from 'k6/data';

// Define scenarios for different test types
export const options = {
    scenarios: {
        // 1. Load Test: Constant arrival rate to check baseline
        load_test: {
            executor: 'ramping-vus',
            startVUs: 0,
            stages: [
                { duration: '30s', target: 20 }, // Ramp up to 20 users
                { duration: '1m', target: 20 },  // Stay at 20 users
                { duration: '30s', target: 0 },  // Ramp down
            ],
            gracefulStop: '30s',
        },
        // 2. Stress Test: Find the breaking point (Optional: comment out to run only load test)
        /*
        stress_test: {
            executor: 'ramping-vus',
            startVUs: 0,
            stages: [
                { duration: '1m', target: 50 },
                { duration: '2m', target: 50 },
                { duration: '1m', target: 100 },
                { duration: '2m', target: 100 },
                { duration: '1m', target: 0 },
            ],
        },
        */
    },
    thresholds: {
        http_req_failed: ['rate<0.01'], // http errors should be less than 1%
        http_req_duration: ['p(95)<500'], // 95% of requests should be below 500ms
    },
};

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5024';

// Sample barcodes from seeded data
const barcodes = ['P1001', 'P2001'];

export default function () {
    const customerId = `user-${__VU}-${__ITER}`;
    const headers = { 'Content-Type': 'application/json' };

    // 1. Create a Basket
    let res = http.post(`${BASE_URL}/basket-api/api/baskets`, JSON.stringify({ customerId }), { headers });
    
    // Check for success or rate limit
    check(res, { 
        'status is 201 or 429': (r) => r.status === 201 || r.status === 429,
        'is not rate limited': (r) => r.status !== 429
    });
    
    if (res.status !== 201) {
        if (res.status === 429) {
            // Respect the rate limit and skip the rest of the flow for this iteration
            // We increase sleep to allow the window to reset
            sleep(1);
            return;
        }
        console.error(`Unexpected ERROR: Failed to create basket: ${res.status} ${res.body}`);
        return;
    }

    const basketId = res.json().id;

    // 2. Add 2-3 items to the basket
    for (let i = 0; i < 2; i++) {
        const barcode = barcodes[Math.floor(Math.random() * barcodes.length)];
        res = http.post(`${BASE_URL}/basket-api/api/baskets/${basketId}/items`, JSON.stringify({
            barcode: barcode,
            quantity: Math.floor(Math.random() * 3) + 1
        }), { headers });
        check(res, { 'item added status is 202': (r) => r.status === 202 });
        sleep(0.5);
    }

    // 3. Get the Basket
    res = http.get(`${BASE_URL}/basket-api/api/baskets/${basketId}`);
    check(res, { 'get basket status is 200': (r) => r.status === 200 });

    // 4. Checkout the Basket
    res = http.post(`${BASE_URL}/basket-api/api/baskets/checkout`, JSON.stringify({ id: basketId }), { headers });
    check(res, { 'checkout status is 202': (r) => r.status === 202 });

    // Sleep longer to respect the "5 requests per 10 seconds" create-basket policy
    // With 20 VUs, we need significant spacing if we don't want to hit 429s constantly.
    sleep(5);
}
