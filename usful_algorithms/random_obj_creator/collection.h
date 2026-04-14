#ifndef RANDOM_OBJ_CREATOR_COLLECTION_H
#define RANDOM_OBJ_CREATOR_COLLECTION_H

#pragma once
#include<string>
#include<iostream>
namespace random_obj_creator
{
    struct ItemMeta {
      std::string name;
      std::string desc;
      std::string image_path;
      int level = 1;
    };
    enum class obj_status
    {
        initial,
        inBackpack,
        onGround,
        inUse,
        destroyed
    };
   class collection
   {
    public:
      // TODO: 构造函数练习清单（可按顺序逐个实现）
      // 1) 默认构造: collection();
      collection();
      // 2) 仅 id 构造: explicit collection(int id);
      collection(int id){
        this->id = id;
        this->current_status = obj_status::initial;
      }
      // 3) id + 状态构造: collection(int id, obj_status st);
      collection(int id, obj_status st): id(id), current_status(st) {
        //do something by st
      };
      // 4) 拷贝构造: collection(const collection& other);
      collection(const collection& other){
        this->id = other.id;
        this->current_status = other.current_status;
        this->meta = other.meta;
      };
      // 4b) 拷贝赋值运算符: collection& operator=(const collection& other);
      collection& operator=(const collection& other){
        if (this == &other) return *this;  // 防自赋值
        this->id = other.id;
        this->current_status = other.current_status;
        this->meta = other.meta;
        return *this;
      };
      // 5) 移动构造: collection(collection&& other) noexcept;
      collection(collection&& other) noexcept {
        this->id = other.id;
        this->current_status = other.current_status;
        this->meta = other.meta;
        // 将 other 重置为默认状态
        other.id = 0;
        other.current_status = obj_status::destroyed;
        other.meta = nullptr;
      };
      // 6) 委托构造: 让简单构造委托到完整构造，减少重复代码
      // 7) 可选: explicit collection(const ItemConfig& cfg);
      ~collection(){};
      //------------------------------------------------------------------------  ↑  构造和析构区域
      std::string get_name() const{
        return meta ? meta->name : "Unknown";
      }
      std::string get_desc() const{
        return meta ? meta->desc : "No description";
      } 
      std::string get_image_path() const{
        return meta ? meta->image_path : "No image";
      } 
      void print_info() const{
        std::cout << "ID: " << id << std::endl;
        std::cout << "Status: " << static_cast<int>(current_status) << std::endl;
        std::cout << "Name: " << get_name() << std::endl;
        std::cout << "Description: " << get_desc() << std::endl;
        std::cout << "Image Path: " << get_image_path() << std::endl;
      }
      void attch_meta(const ItemMeta* meta){
        this->meta = meta;
      }
      //------------------------------------------------------------------------  ↑  public成员函数区域

      private:
        int id = 0;
        obj_status current_status;
        const ItemMeta* meta = nullptr; // 指向物品元数据的只读指针
   };
}

#endif