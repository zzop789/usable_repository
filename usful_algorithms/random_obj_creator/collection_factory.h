#pragma once

#include <random>
#include <stdexcept>
#include <unordered_map>
#include <vector>

#include "collection.h"

namespace random_obj_creator
{
    class CollectionFactory
    {
    public:
        explicit CollectionFactory(const std::unordered_map<int, ItemMeta>& item_meta_db)
            : item_meta_db_(item_meta_db), rng_(std::random_device{}())
        {
            item_ids_.reserve(item_meta_db_.size());
            for (const auto& entry : item_meta_db_) {
                item_ids_.push_back(entry.first);
            }

            if (item_ids_.empty()) {
                throw std::runtime_error("item meta database is empty");
            }
        }

        collection create_random(obj_status st = obj_status::initial)
        {
            std::uniform_int_distribution<size_t> dist(0, item_ids_.size() - 1);
            const int id = item_ids_[dist(rng_)];
            return create_by_id(id, st);
        }

        collection create_by_id(int id, obj_status st = obj_status::initial) const
        {
            auto it = item_meta_db_.find(id);
            if (it == item_meta_db_.end()) {
                throw std::runtime_error("item id not found in database: " + std::to_string(id));
            }

            collection col(id, st);
            col.attch_meta(&it->second);
            return col;
        }

    private:
        const std::unordered_map<int, ItemMeta>& item_meta_db_;
        std::vector<int> item_ids_;
        std::mt19937 rng_;
    };
}
