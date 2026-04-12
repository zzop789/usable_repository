#include <iostream>
#include <map>
#include <unordered_map>
#include <string>

int main() {
    // --- std::map (红黑树，有序) ---
    std::map<std::string, int> wordCount;
    std::string words[] = {"apple", "banana", "apple", "cherry", "banana", "apple"};
    for (const auto& w : words) wordCount[w]++;

    std::cout << "=== std::map (有序) ===\n";
    for (const auto& [word, cnt] : wordCount)
        std::cout << word << ": " << cnt << "\n";

    // 查找
    if (auto it = wordCount.find("banana"); it != wordCount.end())
        std::cout << "banana 出现 " << it->second << " 次\n";

    // --- std::unordered_map (哈希表，无序，O(1) 平均) ---
    std::unordered_map<int, std::string> idName = {
        {1, "Alice"}, {2, "Bob"}, {3, "Charlie"}
    };

    std::cout << "\n=== std::unordered_map ===\n";
    idName[4] = "Dave";
    for (const auto& [id, name] : idName)
        std::cout << "ID " << id << " -> " << name << "\n";

    // 键是否存在：count() 或 contains() (C++20)
    std::cout << "ID 2 存在: " << (idName.count(2) ? "yes" : "no") << "\n";
    std::cout << "ID 9 存在: " << (idName.count(9) ? "yes" : "no") << "\n";

    // 删除
    idName.erase(3);
    std::cout << "删除 ID 3 后，总数: " << idName.size() << "\n";

    return 0;
}
