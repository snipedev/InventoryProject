import { FastifyPluginAsync } from "fastify";

const inventoryRoute: FastifyPluginAsync = async (fastify, _opts) => {
  fastify.get("/inventory", async () => {
    return { message: "inventory is working" };
  });
};

export default inventoryRoute;
