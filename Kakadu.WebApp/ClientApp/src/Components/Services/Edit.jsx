import React from 'react';
import { connect } from 'react-redux';
import { serviceActions, knownRouteActions } from '../../_actions';
import { Formik, Field, ErrorMessage } from 'formik';
import * as Yup from 'yup';
import * as jsonpatch from 'fast-json-patch';
import { List as KnownRoutesList } from '../KnownRoutes';
import TextField from '../../UIComponents/TextField';

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
                    isSubmitting,
                    values }) => (
                    <form onSubmit={handleSubmit}>
                        <div className="form-row">
                            <div className="form-group col-md-8">
                                <TextField
                                    name="name"
                                    placeholder="service name"
                                    label="Name"
                                    renderErrorMessage
                                    touched
                                    errors
                                />
                            </div>
                            <div className="form-group col-md-4">
                                <TextField
                                    name="code"
                                    placeholder="code"
                                    label="Code"
                                    renderErrorMessage
                                    touched
                                    errors
                                />
                            </div>
                        </div>
                        <div className="form-group">
                            <TextField
                                name="address"
                                placeholder="address"
                                label="Address"
                                renderErrorMessage
                                touched
                                errors
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
                            <h4>Known routes</h4>
                            <KnownRoutesList 
                                className="row"
                                knownRoutes={values.knownRoutes} />
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