#include <iostream>
#include <vector>
#include <queue>

// BFS 广度优先搜索
// 图用邻接表表示（无向图）
void bfs(const std::vector<std::vector<int>>& adj, int start, int n) {
    std::vector<bool> visited(n, false);
    std::queue<int> q;

    visited[start] = true;
    q.push(start);

    std::cout << "BFS 遍历顺序: ";
    while (!q.empty()) {
        int node = q.front(); q.pop();
        std::cout << node << " ";

        for (int neighbor : adj[node]) {
            if (!visited[neighbor]) {
                visited[neighbor] = true;
                q.push(neighbor);
            }
        }
    }
    std::cout << "\n";
}

// 求单源最短路（无权图）
std::vector<int> shortestPath(const std::vector<std::vector<int>>& adj, int src, int n) {
    std::vector<int> dist(n, -1);
    std::queue<int> q;
    dist[src] = 0;
    q.push(src);

    while (!q.empty()) {
        int u = q.front(); q.pop();
        for (int v : adj[u]) {
            if (dist[v] == -1) {
                dist[v] = dist[u] + 1;
                q.push(v);
            }
        }
    }
    return dist;
}

int main() {
    // 构建图: 0-1-2-3，0-4，1-4
    int n = 5;
    std::vector<std::vector<int>> adj(n);
    auto addEdge = [&](int u, int v) {
        adj[u].push_back(v);
        adj[v].push_back(u);
    };
    addEdge(0, 1); addEdge(1, 2); addEdge(2, 3);
    addEdge(0, 4); addEdge(1, 4);

    bfs(adj, 0, n);

    auto dist = shortestPath(adj, 0, n);
    std::cout << "从节点 0 出发的最短距离:\n";
    for (int i = 0; i < n; ++i)
        std::cout << "  -> " << i << " : " << dist[i] << "\n";

    return 0;
}
