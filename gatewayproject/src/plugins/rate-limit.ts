// in a plugin, e.g. src/plugins/rate-limit.ts
import fp from "fastify-plugin";
import rateLimit from "@fastify/rate-limit";

export default fp(async (fastify) => {
  await fastify.register(rateLimit, {
    max: 5,
    timeWindow: "1 minute",
  });
});
