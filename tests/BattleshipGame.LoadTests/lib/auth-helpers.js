import http from "k6/http";
import { check } from "k6";
import { config } from "../config.js";

/**
 * Registers a new user via the identity façade and returns their access token.
 * Registration creates both the Keycloak identity and the game profile in one call,
 * so no separate player-creation step is needed.
 * @param {string} username - The username for the new user
 * @returns {string|null} The JWT access token, or null if registration failed
 */
export function registerAndGetToken(username) {
  const payload = JSON.stringify({
    username,
    email: `${username}@loadtest.local`,
    password: "P@ssword123!"
  });
  const params = {
    headers: { "Content-Type": "application/json" },
    tags: { api: "register" }
  };

  const res = http.post(`${config.baseUrl}/api/auth/register`, payload, params);

  const success = check(res, {
    "registered": (r) => r.status === 201,
    "has access token": (r) => !!r.json("accessToken")
  });

  if (!success) {
    console.error(`Failed to register: ${res.status} ${res.body}`);
    return null;
  }

  return res.json("accessToken");
}

/**
 * Builds request headers carrying the JSON content type and bearer token.
 * @param {string} token - The JWT access token
 * @returns {object} Headers object for k6 request params
 */
export function authHeaders(token) {
  return {
    "Content-Type": "application/json",
    Authorization: `Bearer ${token}`
  };
}
