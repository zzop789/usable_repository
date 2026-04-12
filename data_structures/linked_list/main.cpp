#include <iostream>

struct Node {
    int val;
    Node* next;
    Node(int v) : val(v), next(nullptr) {}
};

// 头插法构建链表
Node* buildList(std::initializer_list<int> vals) {
    Node* head = nullptr;
    for (auto it = vals.end(); it != vals.begin(); ) {
        --it;
        Node* node = new Node(*it);
        node->next = head;
        head = node;
    }
    return head;
}

void printList(Node* head) {
    while (head) {
        std::cout << head->val;
        if (head->next) std::cout << " -> ";
        head = head->next;
    }
    std::cout << " -> null\n";
}

// 反转链表（迭代）
Node* reverse(Node* head) {
    Node* prev = nullptr;
    Node* cur  = head;
    while (cur) {
        Node* next = cur->next;
        cur->next  = prev;
        prev = cur;
        cur  = next;
    }
    return prev;
}

// 检测环（Floyd 判圈算法）
bool hasCycle(Node* head) {
    Node* slow = head, * fast = head;
    while (fast && fast->next) {
        slow = slow->next;
        fast = fast->next->next;
        if (slow == fast) return true;
    }
    return false;
}

// 释放链表内存
void freeList(Node* head) {
    while (head) {
        Node* next = head->next;
        delete head;
        head = next;
    }
}

int main() {
    Node* list = buildList({1, 2, 3, 4, 5});

    std::cout << "原链表: ";
    printList(list);

    list = reverse(list);
    std::cout << "反转后: ";
    printList(list);

    std::cout << "有环: " << (hasCycle(list) ? "是" : "否") << "\n";

    // 手动制造环（不能用 freeList，会死循环）
    Node* cycleList = buildList({1, 2, 3});
    cycleList->next->next->next = cycleList->next; // 3 -> 2 形成环
    std::cout << "带环链表 hasCycle: " << (hasCycle(cycleList) ? "是" : "否") << "\n";
    // 清理带环链表
    cycleList->next->next->next = nullptr;
    freeList(cycleList);

    freeList(list);
    return 0;
}
