let svg, simulation, link, node, label;
let graphData = null;
let selectedNode = null;

const COLORS = {
    EventNode: "#33ff33",
    ContextNode: "#00f3ff",
    RuleNode: "#9d00ff",
    EvidenceNode: "#ffaa00",
    ScoreNode: "#ff0055",
    ExplanationNode: "#ffffff",
    DriftWorse: "#ff0000",
    DriftBetter: "#00ff00"
};

// INITIALIZE VIEWPORT
function init() {
    svg = d3.select("#decision-graph-svg");
    const width = document.getElementById('graph-viewport').clientWidth;
    const height = document.getElementById('graph-viewport').clientHeight;

    simulation = d3.forceSimulation()
        .force("link", d3.forceLink().id(d => d.id).distance(120))
        .force("charge", d3.forceManyBody().strength(-400))
        .force("center", d3.forceCenter(width / 2, height / 2))
        .force("collision", d3.forceCollide().radius(60));

    document.getElementById('load-btn').addEventListener('click', loadGraph);
    document.getElementById('diff-btn').addEventListener('click', loadDiff);
    document.getElementById('root-cause-btn').addEventListener('click', loadRootCause);
}

async function loadRootCause() {
    const id = document.getElementById('anomaly-id-input').value;
    if (!id) return notify("Lütfen bir ID girin.");

    try {
        const response = await fetch(`/api/anomalies/${id}/root-cause`);
        if (!response.ok) throw new Error("Causal Analysis yüklenemedi.");
        
        const causalData = await response.json();
        
        // VISUAL HIGHLIGHT: Dim non-causal nodes, glow causal path
        node.style("opacity", d => causalData.causalNodeIds.includes(d.id) ? 1 : 0.1);
        link.style("opacity", l => causalData.causalNodeIds.includes(l.source.id) && causalData.causalNodeIds.includes(l.target.id) ? 1 : 0.05);
        label.style("opacity", d => causalData.causalNodeIds.includes(d.id) ? 1 : 0.1);

        notify(causalData.primaryCause, true);
    } catch (err) {
        notify("HATA: " + err.message);
    }
}

async function loadDiff() {
    const baseId = document.getElementById('anomaly-id-input').value;
    const compareId = document.getElementById('compare-id-input').value;
    
    if (!baseId || !compareId) return notify("Lütfen her iki ID'yi de girin.");

    try {
        const response = await fetch(`/api/anomalies/compare?baseId=${baseId}&compareId=${compareId}`);
        if (!response.ok) throw new Error("Diff verisi alınamadı.");
        
        const diffData = await response.json();
        const graphResponse = await fetch(`/api/anomalies/${compareId}/decision-graph`);
        const graph = await graphResponse.json();

        // MERGE DIFF INTO GRAPH NODES
        graph.nodes.forEach(node => {
            const drift = diffData.ruleDrifts.find(d => `rule-${d.ruleId}` === node.id);
            if (drift) {
                node.drift = drift;
            }
        });

        render(graph);
        notify(diffData.diffSummary, diffData.isMaterialChange);
    } catch (err) {
        notify("HATA: " + err.message);
    }
}

async function loadGraph() {
    const id = document.getElementById('anomaly-id-input').value;
    if (!id) return notify("Lütfen bir ID girin.");

    try {
        const response = await fetch(`/api/anomalies/${id}/decision-graph`);
        if (!response.ok) throw new Error("Graph yüklenemedi.");
        
        graphData = await response.json();
        render(graphData);
        notify("Graph Synchronized", true);
    } catch (err) {
        notify("HATA: " + err.message);
    }
}

function render(data) {
    svg.selectAll("*").remove();

    // Arrow marker
    svg.append("defs").append("marker")
        .attr("id", "arrow")
        .attr("viewBox", "0 -5 10 10")
        .attr("refX", 25)
        .attr("refY", 0)
        .attr("markerWidth", 5)
        .attr("markerHeight", 5)
        .attr("orient", "auto")
        .append("path")
        .attr("d", "M0,-5L10,0L0,5")
        .attr("fill", "#666");

    link = svg.append("g")
        .selectAll("line")
        .data(data.edges)
        .enter().append("line")
        .attr("class", "node-link")
        .attr("marker-end", "url(#arrow)");

    node = svg.append("g")
        .selectAll("circle")
        .data(data.nodes)
        .enter().append("circle")
        .attr("class", "node-circle")
        .attr("r", 20)
        .attr("fill", d => COLORS[d.type] || "#555")
        .call(d3.drag()
            .on("start", dragstarted)
            .on("drag", dragged)
            .on("end", dragended))
        .on("click", (event, d) => selectNode(d));

    label = svg.append("g")
        .selectAll("text")
        .data(data.nodes)
        .enter().append("text")
        .attr("dy", 35)
        .attr("text-anchor", "middle")
        .attr("fill", "#ccc")
        .attr("font-size", "10px")
        .text(d => d.label);

    simulation.nodes(data.nodes).on("tick", ticked);
    simulation.force("link").links(data.edges);
    simulation.alpha(1).restart();
}

function ticked() {
    link
        .attr("x1", d => d.source.x)
        .attr("y1", d => d.source.y)
        .attr("x2", d => d.target.x)
        .attr("y2", d => d.target.y);

    node
        .attr("cx", d => d.x)
        .attr("cy", d => d.y);

    label
        .attr("x", d => d.x)
        .attr("y", d => d.y);
}

// INTERACTION MODEL: SELECT NODE & INSPECTOR
function selectNode(d) {
    selectedNode = d;
    highlightPath(d);
    
    const panel = document.getElementById('inspector-panel');
    panel.classList.remove('hidden');

    document.getElementById('node-type-badge').innerText = d.type;
    document.getElementById('node-label-main').innerText = d.label;
    
    // Metadata Rendering
    const metaContainer = document.getElementById('node-metadata-container');
    metaContainer.innerHTML = "";
    
    // 🔍 DRIFT INFO (If in Diff Mode)
    if (d.drift) {
        const driftBar = document.createElement('div');
        driftBar.className = 'drift-panel';
        const color = d.drift.severityDelta > 0 ? 'red' : 'green';
        driftBar.innerHTML = `
            <div style="color: ${color}; font-weight: bold;">
                DRIFT: ${d.drift.severityDelta > 0 ? 'WORSE' : 'BETTER'} 
                (${Math.abs(d.drift.severityDelta).toFixed(2)})
            </div>
            <div style="font-size: 0.8rem; color: #888;">Status: ${d.drift.driftStatus}</div>
        `;
        metaContainer.appendChild(driftBar);
        metaContainer.appendChild(document.createElement('hr'));
    }

    Object.entries(d.metadata).forEach(([key, val]) => {
        const row = document.createElement('div');
        row.className = 'meta-row';
        row.innerHTML = `<span class='key'>${key}:</span> <span class='val'>${val}</span>`;
        metaContainer.appendChild(row);
    });
}

// 🚀 CRITICAL FEATURE: HIGHLIGHT DECISION PATH
function highlightPath(targetNode) {
    // DFS/BFS ile path bulma logiği eklenebilir. 
    // Basitlik için tüm ilgili linkleri vurguluyoruz:
    link.classed("highlighted", l => l.target.id === targetNode.id || l.source.id === targetNode.id);
    // 🧠 SEMANTIC MEMORY INSIGHTS
    if (window.lastGraphData && window.lastGraphData.similarDecisions && window.lastGraphData.similarDecisions.length > 0) {
        let memoryHtml = "<div style='margin-top:10px; padding:10px; border-left:3px solid #ff00ff; background:rgba(255,0,255,0.05)'>";
        memoryHtml += "<strong>🧠 SEMANTIC RECALL (Benzer Vakalar):</strong><br/>";
        window.lastGraphData.similarDecisions.forEach(sd => {
            memoryHtml += `<div style='font-size:0.85em; margin-top:5px; cursor:pointer' onclick="loadGraph('${sd.anomalyId}')">`;
            memoryHtml += `🔗 %${(sd.hybridScore*100).toFixed(0)} - ${sd.summary.substring(0, 50)}...`;
            memoryHtml += "</div>";
        });
        memoryHtml += "</div>";
        
        console.log("Memory Insights:", window.lastGraphData.similarDecisions);
        notify(memoryHtml, true);
    }
}

// 🚀 RE-RENDER ON RESIZE
window.addEventListener("resize", () => {
    if (window.lastGraphData) render(window.lastGraphData);
});

// HELPERS
function dragstarted(event) { if (!event.active) simulation.alphaTarget(0.3).restart(); event.subject.fx = event.subject.x; event.subject.fy = event.subject.y; }
function dragged(event) { event.subject.fx = event.x; event.subject.fy = event.y; }
function dragended(event) { if (!event.active) simulation.alphaTarget(0); event.subject.fx = null; event.subject.fy = null; }

function closeInspector() { document.getElementById('inspector-panel').classList.add('hidden'); }
function notify(msg, isSuccess=false) {
    const t = document.getElementById('toast');
    t.innerText = msg;
    t.className = isSuccess ? "success" : "error";
    t.classList.remove('hidden');
    setTimeout(() => t.classList.add('hidden'), 3000);
}

window.onload = init;
