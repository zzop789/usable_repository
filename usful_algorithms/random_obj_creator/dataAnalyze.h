#pragma once

#include <fstream>
#include <sstream>
#include <stdexcept>
#include <string>
#include <unordered_map>

#include "collection.h"


	inline std::unordered_map<int, random_obj_creator::ItemMeta> load_item_meta_db_from_csv(const std::string& file_path)
	{
		std::unordered_map<int, random_obj_creator::ItemMeta> db;
		std::ifstream input(file_path);
		if (!input.is_open()) {
			throw std::runtime_error("failed to open csv file: " + file_path);
		}

		std::string line;
		std::getline(input, line);

		while (std::getline(input, line)) {
			if (line.empty()) {
				continue;
			}

			std::stringstream ss(line);
			std::string cell;
			std::string cells[5];

			for (int index = 0; index < 5; ++index) {
				if (!std::getline(ss, cells[index], ',')) {
					throw std::runtime_error("invalid csv row: " + line);
				}
			}

			random_obj_creator::ItemMeta meta;
			meta.name = cells[1];
			meta.desc = cells[2];
			meta.image_path = cells[3];
			meta.level = std::stoi(cells[4]);

			db.emplace(std::stoi(cells[0]), std::move(meta));
		}

		return db;
	}