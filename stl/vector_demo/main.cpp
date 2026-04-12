#include <iostream>
#include <vector>
#include <algorithm>
#include <numeric>

int main() {
    // 基本操作
    std::vector<int> v = {5, 3, 1, 4, 2};
    std::cout << "原始: ";
    for (int x : v) std::cout << x << " ";
    std::cout << "\n";

    // 排序
    std::sort(v.begin(), v.end());
    std::cout << "升序: ";
    for (int x : v) std::cout << x << " ";
    std::cout << "\n";

    // 逆序
    std::sort(v.begin(), v.end(), std::greater<int>());
    std::cout << "降序: ";
    for (int x : v) std::cout << x << " ";
    std::cout << "\n";

    // 查找
    std::sort(v.begin(), v.end()); // 先升序
    auto it = std::lower_bound(v.begin(), v.end(), 3);
    std::cout << "lower_bound(3): 下标 " << (it - v.begin()) << ", 值 " << *it << "\n";

    // 累加
    int sum = std::accumulate(v.begin(), v.end(), 0);
    std::cout << "sum = " << sum << "\n";

    // unique (配合 sort 去重)
    std::vector<int> dup = {1, 2, 2, 3, 3, 3, 4};
    auto endIt = std::unique(dup.begin(), dup.end());
    dup.erase(endIt, dup.end());
    std::cout << "去重后: ";
    for (int x : dup) std::cout << x << " ";
    std::cout << "\n";

    // 动态增删
    std::vector<int> dyn;
    dyn.push_back(10);
    dyn.push_back(20);
    dyn.push_back(30);
    dyn.erase(dyn.begin() + 1); // 删除下标 1
    std::cout << "动态 vector: ";
    for (int x : dyn) std::cout << x << " ";
    std::cout << "\n";

    return 0;
}
