#include <iostream>
#include <vector>

// 二分查找（迭代版）返回下标，未找到返回 -1
int binarySearch(const std::vector<int>& arr, int target) {
    int left = 0, right = (int)arr.size() - 1;
    while (left <= right) {
        int mid = left + (right - left) / 2;
        if (arr[mid] == target) return mid;
        if (arr[mid] < target)  left  = mid + 1;
        else                    right = mid - 1;
    }
    return -1;
}

// 二分查找（递归版）
int binarySearchRecursive(const std::vector<int>& arr, int target, int left, int right) {
    if (left > right) return -1;
    int mid = left + (right - left) / 2;
    if (arr[mid] == target) return mid;
    if (arr[mid] < target)  return binarySearchRecursive(arr, target, mid + 1, right);
    return binarySearchRecursive(arr, target, left, mid - 1);
}

int main() {
    std::vector<int> arr = {2, 5, 8, 12, 16, 23, 38, 56, 72, 91};

    std::cout << "有序数组: ";
    for (int x : arr) std::cout << x << " ";
    std::cout << "\n";

    int targets[] = {23, 100, 2, 91};
    for (int t : targets) {
        int idx = binarySearch(arr, t);
        if (idx != -1) std::cout << "找到 " << t << " 在下标 " << idx << "\n";
        else           std::cout << t << " 不存在\n";
    }

    return 0;
}
