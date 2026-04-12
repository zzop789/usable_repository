#include <iostream>
#include <vector>
#include <unordered_map>

// 方法1：记忆化递归（Top-down DP）
std::unordered_map<int, long long> memo;
long long fibMemo(int n) {
    if (n <= 1) return n;
    if (memo.count(n)) return memo[n];
    return memo[n] = fibMemo(n - 1) + fibMemo(n - 2);
}

// 方法2：迭代 DP（Bottom-up）O(n) 时间 O(n) 空间
long long fibDP(int n) {
    if (n <= 1) return n;
    std::vector<long long> dp(n + 1);
    dp[0] = 0; dp[1] = 1;
    for (int i = 2; i <= n; ++i)
        dp[i] = dp[i - 1] + dp[i - 2];
    return dp[n];
}

// 方法3：空间优化 O(1) 空间
long long fibOpt(int n) {
    if (n <= 1) return n;
    long long a = 0, b = 1;
    for (int i = 2; i <= n; ++i) {
        long long c = a + b;
        a = b; b = c;
    }
    return b;
}

int main() {
    std::cout << "Fibonacci 数列（前 15 项）:\n";
    std::cout << "  记忆化递归: ";
    for (int i = 0; i < 15; ++i) std::cout << fibMemo(i) << " ";
    std::cout << "\n";

    std::cout << "  迭代 DP:    ";
    for (int i = 0; i < 15; ++i) std::cout << fibDP(i) << " ";
    std::cout << "\n";

    std::cout << "  空间优化:   ";
    for (int i = 0; i < 15; ++i) std::cout << fibOpt(i) << " ";
    std::cout << "\n";

    std::cout << "\nfib(40) = " << fibOpt(40) << "\n";
    return 0;
}
