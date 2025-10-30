#!/usr/bin/env python3
import http.server
import socketserver
import os
from urllib.parse import urlparse

class CustomHTTPRequestHandler(http.server.SimpleHTTPRequestHandler):
    def do_GET(self):
        # Parse the path
        parsed_path = urlparse(self.path)
        path = parsed_path.path.rstrip('/')
        
        # Route mapping without .html extensions
        routes = {
            '/secretrecipe': 'secretrecipe.html',
            '/leaderboardadmin': 'leaderboardadmin.html', 
            '/index': 'index.html',
            '': 'index.html'  # Handle root path
        }
        
        # Check if it's a route that needs mapping
        if path in routes:
            # Read and serve the file directly
            file_path = routes[path]
            try:
                with open(file_path, 'rb') as f:
                    content = f.read()
                
                # Send response
                self.send_response(200)
                self.send_header('Content-type', 'text/html')
                self.send_header('Content-Length', str(len(content)))
                self.end_headers()
                self.wfile.write(content)
                return
            except FileNotFoundError:
                self.send_error(404, "File not found")
                return
        
        # For other requests (CSS, JS, images, etc.), use default handler
        return super().do_GET()

if __name__ == "__main__":
    PORT = 8080
    Handler = CustomHTTPRequestHandler
    
    # Try to find an available port
    for port in range(PORT, PORT + 10):
        try:
            with socketserver.TCPServer(("", port), Handler) as httpd:
                print(f"Server running at http://localhost:{port}/")
                print("Available routes:")
                print(f"  http://localhost:{port}/")
                print(f"  http://localhost:{port}/index")  
                print(f"  http://localhost:{port}/secretrecipe")
                print(f"  http://localhost:{port}/leaderboardadmin")
                print(f"  http://localhost:{port}/robots.txt")
                print("\nPress Ctrl+C to stop the server")
                try:
                    httpd.serve_forever()
                except KeyboardInterrupt:
                    print("\nServer stopped.")
                break
        except OSError as e:
            if e.errno == 10048:  # Port already in use
                print(f"Port {port} is in use, trying {port + 1}...")
                continue
            else:
                print(f"Error starting server: {e}")
                break
    else:
        print("Could not find an available port. Please stop other servers or try a different port range.")