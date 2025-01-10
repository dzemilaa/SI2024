import React, { useState, useEffect } from 'react'
import './List.css'
import axios from 'axios'
import {toast} from 'react-toastify'
const List = () => {

  
  const [list,setList] = useState([]);

  const fetchList = async () =>{
    const response = await axios.get(`https://localhost:44376/api/FetchProduct/list`);
    if(response.data.success){
      setList(response.data.data);
    }else{
      toast.error("List is empty.");
    }
  }

  const removeFood = async (foodId) => {
    try {
      setList(prevList => prevList.filter(item => item.productId !== foodId));
      const response = await axios.post(`https://localhost:44376/api/FetchProduct/remove`, { productId: foodId });
      
      if (response.data.success) {
        toast.success("Product deleted successfully.");
      } else {
        toast.error("Error deleting product."); 
      }
      await fetchList();
    } catch (error) {
      toast.error("An error occurred while deleting the product.");
      fetchList();
    }
  };

  useEffect(() => {
  fetchList();
  },[])

  return (
    <div className='list add flex-col'>
      <p>All Foods List</p>
      <div className="list-table">
        <div className="list-table-format title">
          <b>Image</b>
          <b>Name</b>
          <b>Category</b>
          <b>Price</b>
          <b>Action</b>
        </div>
        {list.map((item, index) =>{
          return(
            <div className="list-table-format" key={index}>
              <img src={`https://localhost:44376/images/`+item.image} alt="food" />
              <p>{item.name}</p>
              <p>{item.category}</p>
              <p>{item.price}</p>
              <p onClick={()=>removeFood(item.productId)} className='cursor'>X</p>
            </div>
          )
        })}
      </div>
    </div>
  )
}
export default List
