import { join } from "node:path";
import AutoLoad, { AutoloadPluginOptions } from "@fastify/autoload";
import { FastifyPluginAsync, FastifyServerOptions } from "fastify";
import cors from "@fastify/cors";

export interface AppOptions
  extends FastifyServerOptions, Partial<AutoloadPluginOptions> {}
// Pass --options via CLI arguments in command to enable these options.
const options: AppOptions = {};

const app: FastifyPluginAsync<AppOptions> = async (
  fastify,
  opts,
): Promise<void> => {
  // Place here your custom code!

  fastify.setErrorHandler((err, req, reply) => {
    if (
      err instanceof Error &&
      "code" in err &&
      err.code === "UND_ERR_REQ_TIMEOUT"
    ) {
      return reply.status(504).send({ error: "Gateway Timeout" });
    }
    return reply.send(err);
  });

  // Do not touch the following lines

  // This loads all plugins defined in plugins
  // those should be support plugins that are reused
  // through your application
  // eslint-disable-next-line no-void
  void fastify.register(AutoLoad, {
    dir: join(__dirname, "plugins"),
    options: opts,
  });

  fastify.addHook("preHandler", async (request, reply) => {
    const isLoginRoute =
      request.method === "POST" && request.routeOptions.url === "/login";

    if (isLoginRoute) {
      return;
    }

    await fastify.authenticate(request, reply);
  });

  fastify.register(cors, {
    origin: ["http://localhost:3001"],
  });

  // This loads all plugins defined in routes
  // define your routes in one of these
  // eslint-disable-next-line no-void
  void fastify.register(AutoLoad, {
    dir: join(__dirname, "routes"),
    options: opts,
  });
};

export default app;
export { app, options };
