// 🚀 GRADUAL EXPLANATION LOGIC
async function initDashboard() {
    console.log("SmartWMS | Launch Dashboard Initialized");
    loadStats();
    
    // Auto-sync for simulation if ID is in URL or localStorage
    const lastId = localStorage.getItem('last_anomaly_id');
    if (lastId) {
        syncMirror(lastId);
    }
}

async function loadStats() {
    try {
        const response = await fetch('/api/intelligence/stats');
        const stats = await response.json();
        
        document.getElementById('stats-health').innerText = stats.brainHealthScore;
        document.getElementById('stats-total').innerText = stats.totalAnomaliesProcessed;
        document.getElementById('stats-consistency').innerText = (stats.memoryHitRate * 100).toFixed(0) + "%";
        document.getElementById('stats-time').innerText = stats.averageResolutionTimeMinutes + "m";
    } catch (err) {
        console.error("Stats failed", err);
    }
}

async function syncMirror(id) {
    try {
        const response = await fetch(`/api/intelligence/mirror/${id}`);
        if (!response.ok) return;
        const data = await response.json();

        // 🧠 LEVEL-BASED STORYTELLING
        renderLevel1(data);
        setTimeout(() => renderLevel2(data), 500);
        setTimeout(() => renderLevel3(data), 1000);
        setTimeout(() => renderLevel4(data), 1500);

    } catch (err) {
        console.error("Mirror sync failed", err);
    }
}

function renderLevel1(data) {
    const l1 = document.getElementById('layer-1');
    l1.classList.add('active');
    document.getElementById('l1-content').innerText = 
        data.nodes.some(n => n.type === "ExplanationNode") ? "CRITICAL ANOMALY DETECTED" : "SYSTEM HEALTHY";
}

function renderLevel2(data) {
    const l2 = document.getElementById('layer-2');
    l2.classList.add('active');
    const explNode = data.nodes.find(n => n.type === "ExplanationNode");
    document.getElementById('l2-content').innerText = explNode ? explNode.metadata.Explanation : "All parameters within normal bounds.";
}

function renderLevel3(data) {
    const l3 = document.getElementById('layer-3');
    l3.classList.add('active');
    document.getElementById('l3-content').innerText = `Graph Active: ${data.nodes.length} Reasoning Nodes analyzed across ${data.edges.length} edges.`;
    // Visual indicator of graph complexity
}

function renderLevel4(data) {
    const l4 = document.getElementById('layer-4');
    l4.classList.add('active');
    if (data.similarDecisions && data.similarDecisions.length > 0) {
        const match = data.similarDecisions[0];
        document.getElementById('l4-content').innerText = 
            `RECALL: Matches anomaly ${match.anomalyId.substring(0,8)} with ${(match.hybridScore*100).toFixed(0)}% consistency.`;
    } else {
        document.getElementById('l4-content').innerText = "First-time pattern. Stored as NEW Knowledge.";
    }
}

window.onload = initDashboard;
