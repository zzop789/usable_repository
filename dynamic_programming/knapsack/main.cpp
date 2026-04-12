#include <iostream>
#include <vector>
#include <algorithm>

// 0/1 背包问题
// 给定 n 个物品（重量 w[i]，价值 v[i]），背包容量为 W
// 每个物品最多取 1 次，求最大价值
int knapsack(int W, const std::vector<int>& w, const std::vector<int>& v) {
    int n = w.size();
    // dp[j] = 容量为 j 时的最大价值
    std::vector<int> dp(W + 1, 0);

    for (int i = 0; i < n; ++i) {
        // 逆向遍历防止同一物品被取多次
        for (int j = W; j >= w[i]; --j) {
            dp[j] = std::max(dp[j], dp[j - w[i]] + v[i]);
        }
    }
    return dp[W];
}

// 打印选择方案（二维 DP 回溯）
void knapsackWithTrace(int W, const std::vector<int>& w, const std::vector<int>& v) {
    int n = w.size();
    std::vector<std::vector<int>> dp(n + 1, std::vector<int>(W + 1, 0));

    for (int i = 1; i <= n; ++i)
        for (int j = 0; j <= W; ++j) {
            dp[i][j] = dp[i - 1][j];
            if (j >= w[i - 1])
                dp[i][j] = std::max(dp[i][j], dp[i - 1][j - w[i - 1]] + v[i - 1]);
        }

    // 回溯选择的物品
    std::cout << "最大价值: " << dp[n][W] << "\n";
    std::cout << "选择的物品: ";
    int j = W;
    for (int i = n; i >= 1; --i) {
        if (dp[i][j] != dp[i - 1][j]) {
            std::cout << "物品" << i << "(重" << w[i-1] << ",值" << v[i-1] << ") ";
            j -= w[i - 1];
        }
    }
    std::cout << "\n";
}

int main() {
    // 物品: 重量, 价值
    std::vector<int> weights = {2, 3, 4, 5};
    std::vector<int> values  = {3, 4, 5, 6};
    int capacity = 8;

    std::cout << "背包容量: " << capacity << "\n";
    std::cout << "物品列表:\n";
    for (int i = 0; i < (int)weights.size(); ++i)
        std::cout << "  物品" << i+1 << ": 重=" << weights[i] << " 值=" << values[i] << "\n";
    std::cout << "\n";

    std::cout << "空间优化版最大价值: " << knapsack(capacity, weights, values) << "\n";
    knapsackWithTrace(capacity, weights, values);

    return 0;
}
