import http.server
import socketserver
import os
import functools

PORT = 8000
BUILD_DIR = os.path.dirname(os.path.abspath(__file__))

class WebGLHandler(http.server.SimpleHTTPRequestHandler):
    def end_headers(self):
        self.send_header("Access-Control-Allow-Origin", "*")
        self.send_header("Cross-Origin-Opener-Policy", "same-origin")
        self.send_header("Cross-Origin-Embedder-Policy", "require-corp")
        super().end_headers()

    def guess_type(self, path):
        if path.endswith('.wasm.unityweb') or path.endswith('.wasm'):
            return 'application/wasm'
        if path.endswith('.data.unityweb'):
            return 'application/octet-stream'
        if path.endswith('.js.unityweb') or path.endswith('.js'):
            return 'application/javascript'
        return super().guess_type(path)

    def do_GET(self):
        clean_path = self.path.split('?')[0]
        if clean_path.endswith('.unityweb'):
            actual_file = self.translate_path(clean_path)
            if os.path.exists(actual_file):
                self.send_response(200)
                self.send_header('Content-Type', self.guess_type(clean_path))
                self.send_header('Content-Encoding', 'br')
                self.send_header('Content-Length', str(os.path.getsize(actual_file)))
                self.end_headers()
                with open(actual_file, 'rb') as f:
                    self.copyfile(f, self.wfile)
                return
        super().do_GET()

if __name__ == '__main__':
    handler_factory = functools.partial(WebGLHandler, directory=BUILD_DIR)
    # Allow port reuse
    socketserver.TCPServer.allow_reuse_address = True
    with socketserver.TCPServer(("", PORT), handler_factory) as httpd:
        url = f"http://localhost:{PORT}/index.html"
        print(f"==================================================")
        print(f" PAMANA WebGL Server running at: {url}")
        print(f" Serving directory: {BUILD_DIR}")
        print(f" Press Ctrl+C in terminal to stop.")
        print(f"==================================================")
        try:
            httpd.serve_forever()
        except KeyboardInterrupt:
            print("\nServer stopped.")
