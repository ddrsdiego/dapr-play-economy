import http from 'k6/http';
import { check, group, sleep, trend } from 'k6';

export let options = {
  stages: [
	{ duration: '1m', target: 50 },
    { duration: '1m', target: 0 },
  ],
  thresholds: {
    http_req_duration: ['p(95)<500'],
  },
};

export default function () {
  const url = 'http://localhost:5300/api/v1/customers/8d9f3272';
  const headers = {
    'accept': '*/*',
    'Content-Type': 'application/json',
  };
  const data = JSON.stringify({
    "name": "Diego Dias Ribeiro da Silva"
  });

  group('update customer name', function () {
    const response = http.put(url, data, { headers });

    check(response, {
      'status is 204': (r) => r.status === 204,
    });

    sleep(5); // espera de 100 milissegundos entre as chamadas
  });
}