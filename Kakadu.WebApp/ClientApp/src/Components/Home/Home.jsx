import React from 'react';
import { Link } from 'react-router-dom';
import { connect } from 'react-redux';

import { serviceActions } from '../../_actions';

class Home extends React.Component {
    componentDidMount() {
        this.props.dispatch(serviceActions.getAll());
    }

    render() {
        const { user, services } = this.props;
        return (
            <div className="col-md-6 col-md-offset-3">
                <h3>Services</h3>
                {services.loading && <em>Loading services...</em>}
                {services.error && <span className="text-danger">ERROR: {services.error}</span>}
                {services.items &&
                    <ul>
                        {services.items.map((service, index) =>
                            <li key={index}>
                                {service.code}
                            </li>
                        )}
                    </ul>
                }
                <p>
                    <Link to="/login">Logout</Link>
                </p>
            </div>
        );
    }
}

function mapStateToProps(state) {
    const { services, authentication } = state;
    const { user } = authentication;
    return {
        user,
        services
    };
}

const connectedHomePage = connect(mapStateToProps)(Home);
export { connectedHomePage as Home };