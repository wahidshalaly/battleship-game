# Battleship Game API - K6 Load Tests

This directory contains comprehensive K6 load tests for the Battleship Game API. The tests are designed to validate performance, stability, and scalability under various load conditions.

## 📁 Structure

```
BattleshipGame.LoadTests/
├── config.js                    # Shared configuration and constants
├── lib/
│   └── game-helpers.js         # Reusable helper functions for API calls
├── scenarios/
│   ├── smoke-test.js           # Basic functionality check (1 VU)
│   ├── load-test.js            # Normal load simulation (0-50 VUs)
│   ├── stress-test.js          # High load to find limits (0-200 VUs)
│   ├── spike-test.js           # Sudden traffic burst (10-200 VUs)
│   ├── soak-test.js            # Long-running stability (20 VUs, 30min)
│   └── full-game-simulation.js # Complete game playthrough (10 VUs)
├── package.json
└── README.md
```

## 🚀 Prerequisites

1. **Install K6**: [Installation Guide](https://k6.io/docs/getting-started/installation/)

   **Windows:**

   ```powershell
   winget install k6
   ```

   **macOS:**

   ```bash
   brew install k6
   ```

   **Linux:**

   ```bash
   sudo gpg -k
   sudo gpg --no-default-keyring --keyring /usr/share/keyrings/k6-archive-keyring.gpg --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys C5AD17C747E3415A3642D57D77C6C491D6AC1D69
   echo "deb [signed-by=/usr/share/keyrings/k6-archive-keyring.gpg] https://dl.k6.io/deb stable main" | sudo tee /etc/apt/sources.list.d/k6.list
   sudo apt-get update
   sudo apt-get install k6
   ```

2. **Ensure API is running** at `http://localhost:5000` (or set custom URL via environment variable)

## 🎯 Test Scenarios

### 1. Smoke Test

**Purpose:** Verify basic functionality with minimal load
**Duration:** 1 minute
**VUs:** 1
**When to use:** After deployments, before other tests

```bash
npm run test:smoke
# or
k6 run scenarios/smoke-test.js
```

### 2. Load Test

**Purpose:** Simulate normal expected load
**Duration:** ~10 minutes
**VUs:** 0 → 50 → 0
**When to use:** Regular performance validation

```bash
npm run test:load
# or
k6 run scenarios/load-test.js
```

### 3. Stress Test

**Purpose:** Find the breaking point
**Duration:** ~12 minutes
**VUs:** 0 → 200 → 0
**When to use:** Capacity planning

```bash
npm run test:stress
# or
k6 run scenarios/stress-test.js
```

### 4. Spike Test

**Purpose:** Test sudden traffic bursts
**Duration:** ~7 minutes
**VUs:** 10 → 200 → 10
**When to use:** Validate auto-scaling, circuit breakers

```bash
npm run test:spike
# or
k6 run scenarios/spike-test.js
```

### 5. Soak Test

**Purpose:** Find memory leaks and degradation
**Duration:** 30 minutes
**VUs:** 20 (constant)
**When to use:** Before major releases

```bash
npm run test:soak
# or
k6 run scenarios/soak-test.js
```

### 6. Full Game Simulation

**Purpose:** Simulate complete game flows
**Duration:** 10 minutes
**VUs:** 10
**When to use:** End-to-end performance validation

```bash
npm run test:full-game
# or
k6 run scenarios/full-game-simulation.js
```

## ⚙️ Configuration

### Environment Variables

```bash
# Set custom API base URL
$env:API_BASE_URL="http://localhost:8080"  # PowerShell
export API_BASE_URL="http://localhost:8080"  # Bash

# Run test with custom URL
k6 run -e API_BASE_URL=http://localhost:8080 scenarios/load-test.js
```

### Custom Thresholds

Edit `config.js` to adjust performance thresholds:

```javascript
defaultThresholds: {
    'http_req_failed': ['rate<0.01'],        // Less than 1% errors
    'http_req_duration': ['p(95)<2000'],     // 95th percentile under 2s
    'http_req_duration{api:create_game}': ['p(95)<500'],
}
```

## 📊 Understanding Results

### Key Metrics

- **http_req_duration**: Response time for requests

  - `p(95)`: 95th percentile (95% of requests faster than this)
  - `p(99)`: 99th percentile
  - `avg`: Average response time

- **http_req_failed**: Percentage of failed requests

  - Target: < 1% for most scenarios

- **http_reqs**: Total requests per second (throughput)

- **vus**: Number of virtual users (concurrent connections)

### Sample Output

```
     ✓ player created
     ✓ game created
     ✓ ship placed

     checks.........................: 100.00% ✓ 15000      ✗ 0
     data_received..................: 15 MB   25 kB/s
     data_sent......................: 3.0 MB  5.0 kB/s
     http_req_duration..............: avg=250ms min=50ms med=200ms max=2s p(95)=500ms
       { api:create_game }.........: avg=150ms min=50ms med=120ms max=500ms
       { api:attack }..............: avg=400ms min=100ms med=350ms max=2s
     http_req_failed................: 0.00%   ✓ 0          ✗ 5000
     http_reqs......................: 5000    8.33/s
     vus............................: 50      min=0        max=50
```

## 🎨 Advanced Usage

### Run with Custom VUs and Duration

```bash
k6 run --vus 100 --duration 5m scenarios/load-test.js
```

### Generate HTML Report

```bash
k6 run --out json=results.json scenarios/load-test.js
# Then use k6-reporter or similar tool to generate HTML
```

### Run with Cloud Output (K6 Cloud)

```bash
k6 run --out cloud scenarios/load-test.js
```

### Run All Tests Sequentially

```bash
npm run test:all
```

## 🐛 Troubleshooting

### Connection Refused

- Ensure the API is running: `dotnet run --project src/BattleshipGame.WebAPI`
- Check the base URL in `config.js` or environment variable

### High Error Rate

- Check API logs for errors
- Verify database can handle the load
- Consider scaling API instances

### Timeout Errors

- Increase threshold values in `config.js`
- Check for database query performance
- Review application logs for bottlenecks

## 📈 CI/CD Integration

### GitHub Actions Example

```yaml
name: Load Tests

on: [push]

jobs:
  load-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Run API
        run: |
          dotnet run --project src/BattleshipGame.WebAPI &
          sleep 30
      - name: Install K6
        run: |
          sudo gpg --no-default-keyring --keyring /usr/share/keyrings/k6-archive-keyring.gpg --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys C5AD17C747E3415A3642D57D77C6C491D6AC1D69
          echo "deb [signed-by=/usr/share/keyrings/k6-archive-keyring.gpg] https://dl.k6.io/deb stable main" | sudo tee /etc/apt/sources.list.d/k6.list
          sudo apt-get update
          sudo apt-get install k6
      - name: Run Smoke Test
        working-directory: tests/BattleshipGame.LoadTests
        run: k6 run scenarios/smoke-test.js
```

## 🎯 Performance Targets

| Metric              | Target      | Acceptable | Poor       |
| ------------------- | ----------- | ---------- | ---------- |
| Response Time (p95) | < 500ms     | < 1s       | > 1s       |
| Error Rate          | < 0.1%      | < 1%       | > 1%       |
| Throughput          | > 100 req/s | > 50 req/s | < 50 req/s |
| Availability        | 99.9%       | 99%        | < 99%      |

## 📚 Resources

- [K6 Documentation](https://k6.io/docs/)
- [Load Testing Best Practices](https://k6.io/docs/testing-guides/load-testing/)
- [K6 Examples](https://github.com/grafana/k6-examples)
