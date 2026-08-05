import assert from "node:assert/strict";
import { access, readFile } from "node:fs/promises";
import test from "node:test";

async function render() {
  const workerUrl = new URL("../dist/server/index.js", import.meta.url);
  workerUrl.searchParams.set("test", `${process.pid}-${Date.now()}`);
  const { default: worker } = await import(workerUrl.href);

  return worker.fetch(
    new Request("http://localhost/", {
      headers: { accept: "text/html", host: "localhost" },
    }),
    {
      ASSETS: {
        fetch: async () => new Response("Not found", { status: 404 }),
      },
    },
    {
      waitUntil() {},
      passThroughOnException() {},
    },
  );
}

test("server-renders the HELM launch surface", async () => {
  const response = await render();
  assert.equal(response.status, 200);
  assert.match(response.headers.get("content-type") ?? "", /^text\/html\b/i);

  const html = await response.text();
  assert.match(html, /<title>HELM — Scenario S1<\/title>/i);
  assert.match(html, /Big Brother Is Watching/i);
  assert.match(html, /rel="manifest" href="[^"]*\/site\.webmanifest"/i);
  assert.match(html, /rel="apple-touch-icon"[^>]+apple-touch-icon\.png/i);
  assert.match(html, /rel="icon"[^>]+favicon-32x32\.png/i);
  assert.match(html, /property="og:image"[^>]+og-social\.png/i);
  assert.match(html, /name="twitter:card" content="summary_large_image"/i);
  assert.doesNotMatch(html, /codex-preview|Your site is taking shape/i);
});

test("keeps the launch image and both play modes", async () => {
  const frame = await readFile(
    new URL("../app/GameFrame.tsx", import.meta.url),
    "utf8",
  );

  assert.match(frame, /src="\/og\.png"/);
  assert.match(frame, /PLAY FULLSCREEN/);
  assert.match(frame, /PLAY IN BROWSER/);
  assert.match(frame, /src="\/game\/index\.html"/);
  assert.match(frame, /requestFullscreen/);
});

test("packages the production Unity player", async () => {
  const requiredFiles = [
    "../public/game/index.html",
    "../public/game/Build/WebGL.loader.js",
    "../public/game/Build/WebGL.framework.js.unityweb",
    "../public/game/Build/WebGL.wasm.unityweb",
    "../public/game/Build/WebGL.data.unityweb.part0",
    "../public/game/Build/WebGL.data.unityweb.part1",
    "../public/game/Build/WebGL.data.unityweb.part2",
  ];

  await Promise.all(
    requiredFiles.map((path) => access(new URL(path, import.meta.url))),
  );
});
