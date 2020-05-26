import React from 'react';
import { connect } from 'react-redux';
import { serviceActions } from '../../_actions';
import { Formik, Form as FormikForm, Field, ErrorMessage } from 'formik';
import * as Yup from 'yup';
import * as jsonpatch from 'fast-json-patch';

class Edit extends React.Component {
    componentDidMount() {
        const { serviceCode } = this.props.match.params;
        this.props.dispatch(serviceActions.getByCode(serviceCode));
    }

    render() {
        const requiredFieldMessage = 'This field is required';
        const ServiceSchema = Yup.object({
            code: Yup.string().required(requiredFieldMessage),
            name: Yup.string().required(requiredFieldMessage),
            address: Yup.string().required(requiredFieldMessage),
        });
    
        const { services = {} } = this.props;
        const { item = {} } = services;

        const serviceInitialValues = { 
            id: item.id || '',
            code: item.code || '',
            name: item.name || '',
            address: item.address || '',
            unkownRoutesPassthrough: item.unkownRoutesPassthrough || false,
            knownRoutes: item.knownRoutes || []
        };
        
        return (
            <Formik
                enableReinitialize
                initialValues={serviceInitialValues}
                validationSchema={ServiceSchema}
                onSubmit={
                    (values, { setSubmitting }) => {
                        let diff = jsonpatch.compare(item, values);
                        this.props.dispatch(serviceActions.update(values.code, diff))
                        setSubmitting(false);
                    }
                }>
                {({ errors,
                    touched ,
                    handleSubmit,
                    isSubmitting }) => (
                    <form onSubmit={handleSubmit}>
                        <div className="form-row">
                            <div className="form-group col-md-8">
                                <label htmlFor="name">Name</label>
                                <Field
                                    type="text"
                                    name="name"
                                    placeholder="service name"
                                    className={`form-control ${
                                        touched.name && errors.name ? "is-invalid" : ""
                                    }`}
                                />
                                <ErrorMessage
                                    component="div"
                                    name="name"
                                    className="invalid-feedback"
                                />
                            </div>
                            <div className="form-group col-md-4">
                                <label htmlFor="code">Code</label>
                                <Field
                                    type="text"
                                    name="code"
                                    placeholder="service code"
                                    className={`form-control ${
                                        touched.code && errors.code ? "is-invalid" : ""
                                    }`}
                                    disabled
                                />
                                <ErrorMessage
                                    component="div"
                                    name="code"
                                    className="invalid-feedback"
                                />
                            </div>
                        </div>
                        <div className="form-group">
                            <label htmlFor="address">Address</label>
                            <Field
                                type="text"
                                name="address"
                                placeholder="service address"
                                className={`form-control ${
                                    touched.address && errors.address ? "is-invalid" : ""
                                }`}
                            />
                            <ErrorMessage
                                component="div"
                                name="address"
                                className="invalid-feedback"
                            />
                        </div>
                        <div className="form-group">
                            <div className="form-check">
                                <Field
                                    type="checkbox"
                                    name="unkownRoutesPassthrough"
                                    className="form-check-input"
                                />
                                <label className="form-check-label" htmlFor="unkownRoutesPassthrough">
                                    Allow pass-through for unkown routes
                                </label>
                            </div>
                        </div>
                        <div className="form-group">
                            <button
                                type="submit"
                                className="btn btn-primary btn-block"
                                disabled={isSubmitting}
                            >
                                {isSubmitting ? "Please wait..." : "Save"}
                            </button>
                        </div>
                    </form>
                )}
            </Formik>
        )
    }
};

function mapStateToProps(state) {
    const { services } = state;
    return {
        services
    };
}

const connectedServicesPage = connect(mapStateToProps)(Edit);
export { connectedServicesPage as Edit };