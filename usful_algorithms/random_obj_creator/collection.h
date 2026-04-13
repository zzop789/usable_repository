#pragma
#include<string>
namespace random_obj_creator
{
    enum class status
    {
        initial,
        inBackpack,
        onGround,
        inUse
    };
   class collection
   {
    private:
      int id;
      status current_status;
    public:
      collection(/* args */);
      // TODO: 构造函数练习清单（可按顺序逐个实现）
      // 1) 默认构造: collection();
      // 2) 仅 id 构造: explicit collection(int id);
      // 3) id + 状态构造: collection(int id, status st);
      // 4) 拷贝构造: collection(const collection& other);
      // 5) 移动构造: collection(collection&& other) noexcept;
      // 6) 委托构造: 让简单构造委托到完整构造，减少重复代码
      // 7) 可选: explicit collection(const ItemConfig& cfg);
      ~collection();
    
   };
}