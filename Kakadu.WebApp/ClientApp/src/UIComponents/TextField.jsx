import React from "react";
import { Field, ErrorMessage } from 'formik';

const TextField = props => {
  const {
    name,
    label,
    placeholder,
    renderErrorMessage,
    touched,
    errors,
    disabled,
  } = props;
  return (
    <React.Fragment>
      <label htmlFor={name}>{label}</label>
      <Field
          type="text"
          name={name}
          placeholder={placeholder}
          className={`form-control ${
              touched.code && errors.code ? "is-invalid" : ""
          }`}
          disabled={disabled}
      />
      {renderErrorMessage &&
        <ErrorMessage
            component="div"
            name={name}
            className="invalid-feedback"
        />
      }
    </React.Fragment>
  );
};

export default TextField;