"use client";

import { useEffect, useRef, useState } from "react";

export function GameFrame() {
  const frameShell = useRef<HTMLDivElement>(null);
  const [started, setStarted] = useState(false);
  const [loaded, setLoaded] = useState(false);
  const [failed, setFailed] = useState(false);

  useEffect(() => {
    function receiveGameStatus(event: MessageEvent) {
      if (event.origin !== window.location.origin) return;
      if (event.data?.type === "helm-unity-ready") setLoaded(true);
      if (event.data?.type === "helm-unity-error") setFailed(true);
    }

    window.addEventListener("message", receiveGameStatus);
    return () => window.removeEventListener("message", receiveGameStatus);
  }, []);

  async function enterFullscreen() {
    await frameShell.current?.requestFullscreen();
  }

  function startGame(fullscreen: boolean) {
    setStarted(true);
    if (fullscreen) {
      void frameShell.current?.requestFullscreen().catch(() => {
        // The full-viewport browser version remains playable when native
        // fullscreen is unavailable or denied.
      });
    }
  }

  return (
    <div className="game-frame-shell" ref={frameShell}>
      <div
        className={`web-loading ${started ? "is-starting" : ""} ${
          loaded ? "is-loaded" : ""
        }`}
      >
        <div className="loading-visual" aria-hidden="true">
          <img
            className="loading-art"
            src="/og.png"
            alt=""
            fetchPriority="high"
          />
        </div>
        <div className="loading-actions" aria-live="polite">
          {failed ? (
            <span className="launch-status">
              THE EXAMINATION COULD NOT START — RELOAD TO RETRY
            </span>
          ) : started ? (
            <span className="launch-status">INITIALIZING SCENARIO S1</span>
          ) : (
            <div className="launch-panel">
              <div className="launch-context" aria-hidden="true">
                <span className="launch-kicker">HELM // SCENARIO S1</span>
                <span className="launch-note">SELECT DISPLAY MODE</span>
              </div>
              <div className="launch-buttons">
                <button
                  className="launch-button launch-primary"
                  type="button"
                  onClick={() => startGame(true)}
                >
                  PLAY FULLSCREEN
                </button>
                <button
                  className="launch-button launch-secondary"
                  type="button"
                  onClick={() => startGame(false)}
                >
                  PLAY IN BROWSER
                </button>
              </div>
            </div>
          )}
        </div>
      </div>
      {started && (
        <iframe
          className="game-frame"
          src="/game/index.html"
          title="Play Helm: Scenario S1"
          allow="autoplay; fullscreen; gamepad"
          allowFullScreen
        />
      )}
      {started && (
        <button
          className="fullscreen-button"
          type="button"
          onClick={enterFullscreen}
          aria-label="Enter fullscreen"
        >
          FULLSCREEN
        </button>
      )}
    </div>
  );
}
