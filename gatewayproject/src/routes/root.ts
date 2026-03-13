import { FastifyPluginAsync } from "fastify";

const root: FastifyPluginAsync = async (fastify, opts): Promise<void> => {
  fastify.get("/", async function (request, reply) {
    return { root: true };
  });

  fastify.post("/login", async (request, reply) => {
    const { username, password } = request.body as {
      username: string;
      password: string;
    };

    if (username !== "admin" || password !== "admin123") {
      return reply.status(401).send({ error: "Invalid credentials" });
    }

    const token = (fastify as any).jwt.sign({ user: username, role: "admin" });

    return reply.send({ token });
  });

  fastify.get("/profile", async (request, reply) => {
    return reply.send({ user: (request as any).user });
  });
};

export default root;
