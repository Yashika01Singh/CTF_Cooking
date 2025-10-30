// CookShare Recipe Platform JavaScript

// API Configuration
const API_BASE_URL = 'http://localhost:52112';

// Helper function for API calls
async function apiCall(endpoint, options = {}) {
    const url = `${API_BASE_URL}${endpoint}`;
    const defaultOptions = {
        headers: {
            ...options.headers
        },
        ...options
    };
    
    try {
        const response = await fetch(url, defaultOptions);
        return response;
    } catch (error) {
        console.error('API call failed:', error);
        throw error;
    }
}

// Tab functionality
function showTab(tabName) {
    // Hide all tab contents
    const tabContents = document.querySelectorAll('.tab-content');
    tabContents.forEach(tab => tab.classList.remove('active'));
    
    // Remove active class from all tab buttons
    const tabButtons = document.querySelectorAll('.tab-button');
    tabButtons.forEach(button => button.classList.remove('active'));
    
    // Show selected tab content
    document.getElementById(tabName).classList.add('active');
    
    // Add active class to clicked button
    event.target.classList.add('active');
    
    // Initialize resource graph if graph tab is selected
    if (tabName === 'graph') {
        initResourceGraph();
    }
}

// Initialize the resource graph visualization
function initResourceGraph() {
    const graphContainer = document.getElementById('resourceGraph');
    
    // Clear existing content
    graphContainer.innerHTML = '';
    
    // Add CSS for connections
    const style = document.createElement('style');
    style.textContent = `
        .connection {
            position: absolute;
            height: 2px;
            background: #667eea;
            transform-origin: left center;
            z-index: 1;
        }
        .connection.highlight {
            background: #e53e3e;
            height: 3px;
            animation: pulse 2s infinite;
        }
        @keyframes pulse {
            0%, 100% { opacity: 1; }
            50% { opacity: 0.5; }
        }
    `;
    document.head.appendChild(style);
    
    // Platform architecture data
    const resources = [
        { id: 'storage', name: 'Recipe Storage', type: 'storage', x: 50, y: 50, description: 'cookshare-recipes\nSecure recipe storage' },
        { id: 'vm', name: 'Web Server', type: 'vm', x: 300, y: 100, description: 'CookShare API Host\nHigh availability' },
        { id: 'insights', name: 'Analytics', type: 'insights', x: 500, y: 50, description: 'platform-analytics\nUser engagement tracking' },
        { id: 'api', name: 'Recipe API', type: 'api', x: 300, y: 200, description: 'REST API\nRecipe sharing endpoints' },
        { id: 'webapp', name: 'Web App', type: 'api', x: 100, y: 300, description: 'CookShare Frontend\nUser interface' },
        { id: 'keyvault', name: 'Security Vault', type: 'storage', x: 450, y: 250, description: 'platform-keys\nSecure credential storage' }
    ];
    
    // Connections between resources
    const connections = [
        { from: 'vm', to: 'api', label: 'hosts' },
        { from: 'api', to: 'storage', label: 'writes to' },
        { from: 'api', to: 'insights', label: 'logs to' },
        { from: 'webapp', to: 'api', label: 'calls' },
        { from: 'api', to: 'keyvault', label: 'reads from' },
        { from: 'vm', to: 'insights', label: 'monitors' }
    ];
    
    // Create resource nodes
    resources.forEach(resource => {
        const node = document.createElement('div');
        node.className = `resource-node ${resource.type}`;
        node.style.left = resource.x + 'px';
        node.style.top = resource.y + 'px';
        node.innerHTML = `
            <div style="font-weight: bold; margin-bottom: 4px;">${resource.name}</div>
            <div style="font-size: 10px; opacity: 0.8;">${resource.type.toUpperCase()}</div>
        `;
        node.title = resource.description;
        node.addEventListener('click', () => highlightConnections(resource.id));
        graphContainer.appendChild(node);
    });
    
    // Create connections
    connections.forEach(conn => {
        const fromNode = resources.find(r => r.id === conn.from);
        const toNode = resources.find(r => r.id === conn.to);
        
        if (fromNode && toNode) {
            createConnection(graphContainer, fromNode, toNode, conn.label);
        }
    });
}

// Create a visual connection between two nodes
function createConnection(container, from, to, label) {
    const fromX = from.x + 60; // Center of node
    const fromY = from.y + 40;
    const toX = to.x + 60;
    const toY = to.y + 40;
    
    const length = Math.sqrt(Math.pow(toX - fromX, 2) + Math.pow(toY - fromY, 2));
    const angle = Math.atan2(toY - fromY, toX - fromX) * 180 / Math.PI;
    
    const connection = document.createElement('div');
    connection.className = 'connection';
    connection.style.left = fromX + 'px';
    connection.style.top = fromY + 'px';
    connection.style.width = length + 'px';
    connection.style.transform = `rotate(${angle}deg)`;
    connection.setAttribute('data-from', from.id);
    connection.setAttribute('data-to', to.id);
    connection.title = `${from.name} ${label} ${to.name}`;
    
    container.appendChild(connection);
}

// Highlight connections for a specific node
function highlightConnections(nodeId) {
    const connections = document.querySelectorAll('.connection');
    connections.forEach(conn => {
        if (conn.getAttribute('data-from') === nodeId || conn.getAttribute('data-to') === nodeId) {
            conn.classList.add('highlight');
        } else {
            conn.classList.remove('highlight');
        }
    });
    
    // Remove highlights after 3 seconds
    setTimeout(() => {
        connections.forEach(conn => conn.classList.remove('highlight'));
    }, 3000);
}

// File upload functionality
let selectedFile = null;

// Recipe form submission
document.addEventListener('DOMContentLoaded', function() {
    initializeFileUpload();
    
    const recipeForm = document.getElementById('recipeForm');
    if (recipeForm) {
        recipeForm.addEventListener('submit', async function(e) {
            e.preventDefault();
            
            const messageDiv = document.getElementById('submitMessage');
            const username = document.getElementById('username').value.trim();
            
            if (!username) {
                messageDiv.innerHTML = '<div class="message error">❌ Please enter a username</div>';
                return;
            }
            
            if (!selectedFile) {
                messageDiv.innerHTML = '<div class="message error">❌ Please select a recipe file</div>';
                return;
            }
            
            messageDiv.innerHTML = '<div class="message">⏳ Uploading and validating recipe...</div>';
            
            const formData = new FormData();
            formData.append('username', username);
            formData.append('recipeFile', selectedFile);
            
            try {
                const response = await apiCall('/api/validaterecipe', {
                    method: 'POST',
                    body: formData
                });
                
                const result = await response.json();
                
                if (response.ok && result.success) {
                    messageDiv.innerHTML = `
                        <div class="message success">
                            🎉 Recipe uploaded and validated successfully!
                            <br>📊 Your score: <strong>${result.data.score} points</strong>
                            <br>� File: <strong>${selectedFile.name}</strong>
                            <br>�🔗 Storage URL: <a href="${result.data.storageUrl}" target="_blank">View in storage</a>
                        </div>
                    `;
                    
                    // Reset form
                    resetFileUpload();
                    recipeForm.reset();
                } else {
                    messageDiv.innerHTML = `
                        <div class="message error">
                            ❌ ${result.message || 'Failed to upload recipe'}
                        </div>
                    `;
                }
            } catch (error) {
                messageDiv.innerHTML = `
                    <div class="message error">
                        ❌ Error uploading recipe. Please try again.
                    </div>
                `;
                console.error('Recipe upload error:', error);
            }
        });
    }
});

// Initialize file upload functionality
function initializeFileUpload() {
    const dropZone = document.getElementById('fileDropZone');
    const fileInput = document.getElementById('recipeFile');
    const browseBtn = document.getElementById('browseBtn');
    const removeBtn = document.getElementById('removeFile');
    const submitBtn = document.getElementById('submitBtn');
    
    // Browse button click
    browseBtn.addEventListener('click', () => {
        fileInput.click();
    });
    
    // File input change
    fileInput.addEventListener('change', (e) => {
        if (e.target.files.length > 0) {
            handleFileSelect(e.target.files[0]);
        }
    });
    
    // Remove file button
    removeBtn.addEventListener('click', resetFileUpload);
    
    // Drag and drop events
    dropZone.addEventListener('dragover', (e) => {
        e.preventDefault();
        dropZone.classList.add('dragover');
    });
    
    dropZone.addEventListener('dragleave', (e) => {
        e.preventDefault();
        if (!dropZone.contains(e.relatedTarget)) {
            dropZone.classList.remove('dragover');
        }
    });
    
    dropZone.addEventListener('drop', (e) => {
        e.preventDefault();
        dropZone.classList.remove('dragover');
        
        const files = e.dataTransfer.files;
        if (files.length > 0) {
            handleFileSelect(files[0]);
        }
    });
}

// Handle file selection
function handleFileSelect(file) {
    // Validate file size (10MB max)
    const maxSize = 10 * 1024 * 1024;
    if (file.size > maxSize) {
        document.getElementById('submitMessage').innerHTML = 
            '<div class="message error">❌ File size exceeds 10MB limit</div>';
        return;
    }
    
    // Validate file type
    const allowedTypes = ['.txt', '.pdf', '.doc', '.docx', '.json'];
    const fileExt = '.' + file.name.split('.').pop().toLowerCase();
    if (!allowedTypes.includes(fileExt)) {
        document.getElementById('submitMessage').innerHTML = 
            '<div class="message error">❌ Unsupported file type. Please use TXT, PDF, DOC, DOCX, or JSON files.</div>';
        return;
    }
    
    selectedFile = file;
    
    // Update UI
    document.getElementById('fileDropZone').style.display = 'none';
    const selectedFileDiv = document.getElementById('selectedFile');
    selectedFileDiv.style.display = 'flex';
    selectedFileDiv.querySelector('.file-name').textContent = `${file.name} (${formatFileSize(file.size)})`;
    
    // Enable submit button
    document.getElementById('submitBtn').disabled = false;
    
    // Clear any previous messages
    document.getElementById('submitMessage').innerHTML = '';
}

// Reset file upload
function resetFileUpload() {
    selectedFile = null;
    document.getElementById('recipeFile').value = '';
    document.getElementById('fileDropZone').style.display = 'block';
    document.getElementById('selectedFile').style.display = 'none';
    document.getElementById('submitBtn').disabled = true;
}

// Format file size for display
function formatFileSize(bytes) {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
}

// Simple storage access function (for demo purposes)
function accessStorageDemo(url) {
    window.open(url, '_blank');
}

// Update leaderboard from server
async function updateLeaderboard() {
    try {
        const response = await apiCall('/api/leaderboard');
        const result = await response.json();
        
        if (response.ok && result.data) {
            const leaderboardContainer = document.querySelector('.leaderboard-container');
            leaderboardContainer.innerHTML = '';
            
            result.data.forEach((user, index) => {
                const isTopChef = user.username === 'chef-maria' || user.username === 'top-chef';
                const item = document.createElement('div');
                item.className = `leaderboard-item ${isTopChef ? 'top-chef' : ''}`;
                item.innerHTML = `
                    <span class="rank">${user.rank || index + 1}</span>
                    <span class="username" ${isTopChef ? 'id="topUser"' : ''}>${user.username}</span>
                    <span class="score">${user.score} points</span>
                    ${isTopChef ? '<span class="hint">⭐ Featured Chef - Check out their amazing recipes!</span>' : ''}
                `;
                leaderboardContainer.appendChild(item);
            });
            
            // Re-add click handler for featured chef
            const topUserElement = document.getElementById('topUser');
            if (topUserElement) {
                topUserElement.addEventListener('click', function() {
                    alert('⭐ Featured Chef: This chef has shared some incredible recipes! Check out their profile for inspiration.');
                });
            }
        }
    } catch (error) {
        console.error('Failed to update leaderboard:', error);
    }
}

// Initialize page
document.addEventListener('DOMContentLoaded', function() {
    // Load initial leaderboard
    updateLeaderboard();
    
    // Add click handlers after initial load
    setTimeout(() => {
        const topUserElement = document.getElementById('topUser');
        if (topUserElement) {
            topUserElement.addEventListener('click', function() {
                alert('⭐ Featured Chef: This chef is known for authentic Italian cuisine and family recipes passed down through generations!');
            });
        }
    }, 1000);
});