#include <iostream>
#include <vector>

// DFS 深度优先搜索（递归）
void dfsRecursive(const std::vector<std::vector<int>>& adj,
                  std::vector<bool>& visited, int node) {
    visited[node] = true;
    std::cout << node << " ";
    for (int neighbor : adj[node]) {
        if (!visited[neighbor])
            dfsRecursive(adj, visited, neighbor);
    }
}

// 检测无向图是否有环（DFS）
bool hasCycleDFS(const std::vector<std::vector<int>>& adj,
                 std::vector<bool>& visited, int node, int parent) {
    visited[node] = true;
    for (int neighbor : adj[node]) {
        if (!visited[neighbor]) {
            if (hasCycleDFS(adj, visited, neighbor, node)) return true;
        } else if (neighbor != parent) {
            return true; // 发现已访问且不是父节点 => 有环
        }
    }
    return false;
}

int main() {
    int n = 6;
    std::vector<std::vector<int>> adj(n);
    auto addEdge = [&](int u, int v) {
        adj[u].push_back(v);
        adj[v].push_back(u);
    };
    addEdge(0, 1); addEdge(0, 2);
    addEdge(1, 3); addEdge(1, 4);
    addEdge(2, 5);

    std::vector<bool> visited(n, false);
    std::cout << "DFS 遍历（递归）: ";
    dfsRecursive(adj, visited, 0);
    std::cout << "\n";

    // 检测环
    std::vector<std::vector<int>> adj2(4);
    auto addEdge2 = [&](int u, int v) { adj2[u].push_back(v); adj2[v].push_back(u); };
    addEdge2(0, 1); addEdge2(1, 2); addEdge2(2, 0); // 三角形 => 有环

    std::vector<bool> vis2(4, false);
    bool cycle = hasCycleDFS(adj2, vis2, 0, -1);
    std::cout << "图2 是否有环: " << (cycle ? "是" : "否") << "\n";

    return 0;
}
