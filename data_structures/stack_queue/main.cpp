#include <iostream>
#include <stack>
#include <queue>
#include <string>

// 用 stack 验证括号匹配
bool isBalanced(const std::string& s) {
    std::stack<char> st;
    for (char c : s) {
        if (c == '(' || c == '[' || c == '{') {
            st.push(c);
        } else if (c == ')' || c == ']' || c == '}') {
            if (st.empty()) return false;
            char top = st.top(); st.pop();
            if ((c == ')' && top != '(') ||
                (c == ']' && top != '[') ||
                (c == '}' && top != '{')) return false;
        }
    }
    return st.empty();
}

// 用 queue 模拟任务调度
void taskScheduler(std::queue<std::string>& tasks) {
    std::cout << "任务调度顺序: ";
    while (!tasks.empty()) {
        std::cout << tasks.front() << " ";
        tasks.pop();
    }
    std::cout << "\n";
}

int main() {
    // --- Stack ---
    std::cout << "=== Stack ===\n";
    std::stack<int> s;
    s.push(1); s.push(2); s.push(3);
    std::cout << "栈顶: " << s.top() << "\n";
    s.pop();
    std::cout << "pop 后栈顶: " << s.top() << "\n";

    // 括号匹配
    std::string exprs[] = {"([]{})", "([)]", "{{}}"};
    for (const auto& e : exprs)
        std::cout << e << " => " << (isBalanced(e) ? "合法" : "非法") << "\n";

    // --- Queue ---
    std::cout << "\n=== Queue ===\n";
    std::queue<std::string> q;
    q.push("Task-A"); q.push("Task-B"); q.push("Task-C");
    taskScheduler(q);

    // --- Priority Queue (最大堆) ---
    std::cout << "\n=== Priority Queue (最大堆) ===\n";
    std::priority_queue<int> pq;
    for (int x : {3, 1, 4, 1, 5, 9, 2, 6}) pq.push(x);
    std::cout << "按优先级出队: ";
    while (!pq.empty()) { std::cout << pq.top() << " "; pq.pop(); }
    std::cout << "\n";

    return 0;
}
