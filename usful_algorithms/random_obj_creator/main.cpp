#pragma once
#include <iostream>
#include <vector>
#include "collection_factory.h"
#include "dataAnalyze.h"

int main(){
    auto item_meta_db = load_item_meta_db_from_csv("items.csv");
    random_obj_creator::CollectionFactory factory(item_meta_db);

    std::vector<random_obj_creator::collection> collections;
    std::vector<random_obj_creator::collection> collections_on_the_ground;
    int num_postions;
    std::cin>>num_postions;
    for(int i=0;i<num_postions;i++){
        collections.push_back(factory.create_random());
    }
    for(int i=0;i<collections.size();i++){
        std::cout<<"postion_"<<i<<":"<<std::endl;
        collections[i].print_info();
        std::cout<<"postion "<<i<<" been placed collection"<<collections[i].get_name()<<std::endl;
        std::cout<<"------------------------------"<<std::endl;
    }
    
    return 0;
}   