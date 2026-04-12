#include <iostream>
#include <vector>
#include <algorithm>

// 冒泡排序 O(n^2)
void bubbleSort(std::vector<int>& arr) {
    int n = arr.size();
    for (int i = 0; i < n - 1; ++i) {
        bool swapped = false;
        for (int j = 0; j < n - i - 1; ++j) {
            if (arr[j] > arr[j + 1]) {
                std::swap(arr[j], arr[j + 1]);
                swapped = true;
            }
        }
        if (!swapped) break; // 已有序，提前退出
    }
}

void printArr(const std::vector<int>& arr) {
    for (int x : arr) std::cout << x << " ";
    std::cout << "\n";
}

int main() {
    std::vector<int> arr = {64, 34, 25, 12, 22, 11, 90};

    std::cout << "原始数组: ";
    printArr(arr);

    bubbleSort(arr);

    std::cout << "排序结果: ";
    printArr(arr);

    return 0;
}
