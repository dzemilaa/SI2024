import React, { useState } from 'react';
import './Add.css';
import '../../index.css';
import { assets } from '../../assets/assets';
import axios from 'axios';
import { toast } from 'react-toastify';

const Add = () => {
  const [image, setImage] = useState(null);
  const [data, setData] = useState({
    name: "",
    description: "",
    price: "",
    category: "Salad"
  });


  const onChangeHandler = (event) => {
    const name = event.target.name;
    const value = event.target.value;
    setData(prevData => ({ ...prevData, [name]: value }));
  };


  const onSubmitHandler = async (event) => {
    event.preventDefault();


  
    if (!data.name || !data.description || !data.price || !image) {
      if (!data.name) {
        toast.error("Please fill in the product name.");
      } 
      if (!data.description) {
        toast.error("Please fill in the product description.");
      } 
      if (!data.price) {
        toast.error("Please fill in the product price.");
      }
      if (!image) {
        toast.error("Please upload an image.");
      }
      return;
    }
    
    if (isNaN(data.price) || data.price.trim() === "") {
      toast.error("Please enter a valid price. Price must be a number.");
      return;
    }

    const allowedImageTypes = ['image/jpeg', 'image/jpg', 'image/png'];
    if (image && !allowedImageTypes.includes(image.type)) {
      toast.error("Please upload a valid image (JPG, JPEG, PNG).");
      return;
    }

    const formData = new FormData();
    formData.append("name", data.name);
    formData.append("description", data.description);
    formData.append("price", Number(data.price));
    formData.append("category", data.category);
    formData.append("image", image);

 
    console.log("Name: ", data.name);
    console.log("Description: ", data.description);
    console.log("Price: ", data.price);
    console.log("Category: ", data.category);
    console.log("Image: ", image);

    try {

      const response = await axios.post(`https://localhost:44376/api/Product/add`, formData);

      if (response.status === 200) {
      
        setData({
          name: "",
          description: "",
          price: "",
          category: "Salad"
        });
        setImage(null);
        toast.success("Product added successfully");
      } else {
        alert("Failed to add product");
      }
    } catch (error) {
      console.error("Error while adding product:", error);
      alert("An error occurred while adding the product.");
    }
  };

  return (
    <div className='add'>
      <form className='flex-col' onSubmit={onSubmitHandler} noValidate>
        <div className='add-img-upload flex-col'>
          <p>Upload Image</p>
          <label htmlFor="image">
            <img
              src={image ? URL.createObjectURL(image) : assets.upload_area}
              alt="Upload"
            />
          </label>
          <input
            onChange={(e) => setImage(e.target.files[0])}
            type="file"
            id="image"
            hidden
            required
          />
        </div>

        <div className="add-product-name flex-col">
          <p>Product Name</p>
          <input
            onChange={onChangeHandler}
            value={data.name}
            type="text"
            name="name"
            placeholder="Type here"
            required
          />
        </div>
        <div className="add-product-description flex-col">
          <p>Product description</p>
          <textarea
            onChange={onChangeHandler}
            value={data.description}
            name="description"
            rows="6"
            placeholder="Write content here"
            required
          />
        </div>

        <div className="add-category-price">
          <div className="add-category flex-col">
            <p>Product category</p>
            <select
              onChange={onChangeHandler}
              name="category"
              value={data.category}
            >
              <option value="Salad">Salad</option>
              <option value="Rolls">Rolls</option>
              <option value="Deserts">Deserts</option>
              <option value="Sandwich">Sandwich</option>
              <option value="Cake">Cake</option>
              <option value="Pasta">Pasta</option>
              <option value="Noodles">Noodles</option>
            </select>
          </div>

          <div className="add-price flex-col">
            <p>Product price</p>
            <input
              onChange={onChangeHandler}
              value={data.price}
              type="number"
              name="price"
              placeholder="$20"
              required
            />
          </div>
        </div>

        <button type="submit" className="add-btn">ADD</button>
      </form>
    </div>
  );
};

export default Add;
