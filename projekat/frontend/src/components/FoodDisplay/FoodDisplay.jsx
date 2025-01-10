import React, { useContext } from 'react';
import './FoodDisplay.css';
import { StoreContext } from '../../context/StoreContext';
import FoodItem from '../FoodItem/FoodItem';

const FoodDisplay = ({ category }) => {
  const { food_list } = useContext(StoreContext);

  if (!food_list || food_list.length === 0) {
    return <p className="food-display-empty">No food items available.</p>;
  }

  return (
    <div className='food-display' id='food-display'>
      <h2>Top dishes near you</h2>
      <div className="food-display-list">
        {food_list.map((item) => {
          // Proverite da li je kategorija "All" ili odgovara trenutnom artiklu
          if (category === "All" || category === item.category) {
            return (
              <FoodItem
                key={item._id} // Koristimo `_id` jer je dosledno u StoreContext
                id={item._id}
                name={item.name}
                description={item.description}
                price={item.price}
                image={item.image}
              />
            );
          }
          return null; // Vraćamo `null` ako uslov nije ispunjen
        })}
      </div>
    </div>
  );
};

export default FoodDisplay;
